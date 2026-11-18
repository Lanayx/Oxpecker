namespace Oxpecker

[<AutoOpen>]
module ModelValidation =

    open System
    open System.Collections
    open System.Collections.Concurrent
    open System.Collections.Generic
    open System.ComponentModel
    open System.ComponentModel.DataAnnotations
    open System.Linq.Expressions
    open System.Reflection
    open System.Runtime.CompilerServices
    open System.Text
    open Microsoft.AspNetCore.Http
    open Microsoft.FSharp.Reflection
    open Oxpecker.ViewEngine.Tools

    /// <summary>
    /// Stops model validation from descending into the value of a property: the object, or the collection items, stored in it are not validated.
    /// Validation attributes applied to the property itself are still evaluated.
    /// </summary>
    [<Sealed; AttributeUsage(AttributeTargets.Property)>]
    type SkipNestedValidationAttribute() =
        inherit Attribute()

    type ValidationErrors(errors: ResizeArray<ValidationResult>) =
        let errorDict =
            lazy
                (let dict = Dictionary<string, ResizeArray<string | null>>()
                 for error in errors do
                     for memberName in error.MemberNames do
                         match dict.TryGetValue(memberName) with
                         | true, value -> value.Add(error.ErrorMessage)
                         | false, _ ->
                             let arrayList = ResizeArray(1)
                             arrayList.Add(error.ErrorMessage)
                             dict[memberName] <- arrayList
                 dict)
        /// <summary>
        /// Get all validation results for a model.
        /// </summary>
        member this.All: ValidationResult seq = errors
        /// <summary>
        /// Get all error messages for a specific model field (could to be used with `nameof` funciton).
        /// Fields of nested objects are addressed by their path from the model, e.g. <c>Address.Street</c> or <c>Items[0].Name</c>.
        /// </summary>
        member this.ErrorMessagesFor(name) : seq<string | null> =
            match errorDict.Value.TryGetValue(name) with
            | true, value -> value
            | false, _ -> Array.empty

    type InvalidModel<'T> = 'T * ValidationErrors

    /// <summary>
    /// Model wrapper for convenient usage with Oxpecker.ViewEngine
    /// </summary>
    [<RequireQualifiedAccess>]
    type ModelState<'T> =
        /// Could be used for "create" views.
        | Empty
        /// Could be used for "edit" views.
        | Valid of 'T
        /// Could be used for displaying errors in "create" and "edit" views.
        | Invalid of InvalidModel<'T>
        /// <summary>
        /// Pass an accessor function to get the string value of a model field (could be used with shorthand lambda).
        /// </summary>
        member this.Value(f: 'T -> string | null) =
            match this with
            | Empty -> null
            | Valid model -> f model
            | Invalid(model, _) -> f model

        /// <summary>
        /// Pass an accessor function to get the boolean value of a model field (could be used with shorthand lambda).
        /// </summary>
        member this.BoolValue(f: 'T -> bool) =
            match this with
            | Empty -> false
            | Valid model -> f model
            | Invalid(model, _) -> f model

    [<RequireQualifiedAccess>]
    type ModelValidationResult<'T> =
        | Valid of 'T
        | Invalid of InvalidModel<'T>

    /// Recursive validation of an object graph on top of <see cref="System.ComponentModel.DataAnnotations.Validator"/>.
    module private ObjectGraph =

        /// Reads a property of the object passed to it.
        type Getter = Func<obj, obj | null>

        type PropertyMetadata = {
            Name: string
            Get: Getter
            Validators: ValidationAttribute[]
            Display: DisplayAttribute | null
            /// Whether the property value may contain something to validate.
            Traverse: bool
        }

        type ValueKind =
            /// Nothing to validate inside: simple values, framework types and collections of those.
            | Leaf
            /// An option, validated as the value it holds.
            | Optional of value: Getter
            /// A voption, validated as the value it holds.
            | ValueOptional of isSome: Getter * value: Getter
            /// A dictionary entry: only its value is validated, the key becomes a part of the path.
            | KeyValue of key: Getter * value: Getter
            /// A struct union having several cases, validated as the case of the value (cases are ordered by tag).
            /// A struct has the fields of all the cases at once, and those of the other cases hold default values,
            /// which are not a part of the value.
            | StructUnion of tag: Func<obj, int> * cases: TypeMetadata[]
            /// An object with properties, type-level validation and/or items to validate.
            | Composite

        and TypeMetadata = {
            Kind: ValueKind
            Properties: PropertyMetadata[]
            TypeValidators: ValidationAttribute[]
            TypeDisplay: DisplayAttribute | null
            HasItems: bool
            IsReferenceType: bool
        }

        let leaf = {
            Kind = Leaf
            Properties = [||]
            TypeValidators = [||]
            TypeDisplay = null
            HasItems = false
            IsReferenceType = false
        }

        let cache = ConcurrentDictionary<Type, TypeMetadata>()

        let isGeneric (definition: Type) (t: Type) =
            t.IsGenericType && t.GetGenericTypeDefinition() = definition

        /// Types of the System.* and Microsoft.* namespaces are not models: their properties are never traversed.
        let isFrameworkType (t: Type) =
            match t.Namespace with
            | null -> false
            | ns ->
                ns = "System"
                || ns.StartsWith("System.", StringComparison.Ordinal)
                || ns.StartsWith("Microsoft.", StringComparison.Ordinal)

        /// Whether the property is a field of a record or of a union case. A field shared by several cases
        /// of a struct union is mapped once for each of them.
        let isField (p: PropertyInfo) =
            p.GetCustomAttributes<CompilationMappingAttribute>()
            |> Seq.exists(fun mapping -> mapping.SourceConstructFlags = SourceConstructFlags.Field)

        /// Attributes are found the way Validator does it, through TypeDescriptor: it also knows the ones registered
        /// for a type, e.g. by AssociatedMetadataTypeTypeDescriptionProvider for a class holding the metadata of a model.
        let typeAttributes (t: Type) : Attribute[] = [|
            for attribute in TypeDescriptor.GetAttributes t do
                attribute
        |]

        /// Attributes of the property itself: its descriptor also has the ones of the property type.
        let propertyAttributes (descriptor: PropertyDescriptor) : Attribute[] =
            let ofPropertyType = typeAttributes descriptor.PropertyType
            [|
                for attribute in descriptor.Attributes do
                    // By reference, as an equal attribute may be applied to the property as well.
                    if
                        not(
                            ofPropertyType
                            |> Array.exists(fun other -> obj.ReferenceEquals(other, attribute))
                        )
                    then
                        attribute
            |]

        /// Whether the descriptor reads the property it stands for, as the descriptors of the framework do: compiled
        /// code can read such a property instead. A descriptor of another TypeDescriptionProvider may take the value
        /// from anywhere, so it is asked for it.
        let readsProperty (descriptor: PropertyDescriptor) =
            let assembly = descriptor.GetType().Assembly
            assembly = typeof<TypeDescriptor>.Assembly
            || assembly = typeof<Validator>.Assembly

        let ofType<'T when 'T :> Attribute> (attributes: Attribute[]) : 'T[] =
            attributes
            |> Array.choose(fun attribute ->
                match attribute with
                | :? 'T as found -> Some found
                | _ -> None)

        /// Pointers, references and ref structs are not objects: such a value is neither traversed nor read by compiled code.
        let isNotObject (t: Type) =
            t.IsPointer || t.IsByRef || t.IsByRefLike

        /// Compiles a reader of the property. Properties are read for every validated object,
        /// and compiled code does it several times faster than PropertyInfo.GetValue.
        let getter (t: Type) (p: PropertyInfo) : Getter =
            if isNotObject p.PropertyType then
                // Cannot be a part of an expression tree, but reflection is able to read it.
                Getter(fun instance -> p.GetValue instance)
            else
                let instance = Expression.Parameter(typeof<obj>)
                let value = Expression.Property(Expression.Convert(instance, t), p)
                Expression.Lambda<Getter>(Expression.Convert(value, typeof<obj>), instance).Compile()

        /// Item types of a collection type, empty when they are not known statically.
        let itemTypes (t: Type) = [|
            if t.IsArray then
                match t.GetElementType() with
                | null -> ()
                | elementType -> elementType
            else
                for i in t.GetInterfaces() do
                    if isGeneric typedefof<IEnumerable<_>> i then
                        i.GetGenericArguments()[0]
        |]

        /// What a type is known by while it is analysed. For a generic model type it is the definition, so that
        /// its instances are never analysed inside each other: Nest<'T> = { Inner: Nest<'T list> } refers
        /// to another instance on every level, and following them all would never end.
        let analysisKey (t: Type) =
            if t.IsGenericType && not(isFrameworkType t) then
                t.GetGenericTypeDefinition()
            else
                t

        let rec metadataFor (visiting: HashSet<Type>) (t: Type) =
            match cache.TryGetValue t with
            | true, metadata -> metadata
            | _ ->
                let key = analysisKey t
                visiting.Add key |> ignore
                let metadata = createMetadata visiting t
                visiting.Remove key |> ignore
                cache[t] <- metadata
                metadata

        /// Whether a value of the declared type can never contain anything to validate. Only sealed types are decided
        /// statically, the rest depends on the runtime type. A type which is still being analysed (a recursive one)
        /// is not a leaf: it is analysed when a value of it is met.
        and isStaticLeaf (visiting: HashSet<Type>) (t: Type) =
            t.IsSealed
            && not(visiting.Contains(analysisKey t))
            && (metadataFor visiting t).Kind.IsLeaf

        and createMetadata (visiting: HashSet<Type>) (t: Type) =
            let property (name: string) = getter t (nonNull(t.GetProperty name))
            // The kind is created on demand: nothing is compiled for a leaf.
            let holding index (kind: unit -> ValueKind) =
                if isStaticLeaf visiting (t.GetGenericArguments()[index]) then
                    leaf
                else
                    { leaf with Kind = kind() }

            if isGeneric typedefof<Nullable<_>> t then
                // Only matters as a declared type: a boxed Nullable is either null or the value it holds.
                holding 0 (fun () -> Composite)
            elif isGeneric typedefof<option<_>> t then
                holding 0 (fun () -> Optional(property "Value"))
            elif isGeneric typedefof<voption<_>> t then
                holding 0 (fun () -> ValueOptional(property "IsSome", property "Value"))
            elif isGeneric typedefof<KeyValuePair<_, _>> t then
                holding 1 (fun () -> KeyValue(property "Key", property "Value"))
            elif t = typeof<DictionaryEntry> then
                // An entry of a non-generic dictionary such as Hashtable: the type of its value is only known at runtime.
                {
                    leaf with
                        Kind = KeyValue(property "Key", property "Value")
                }
            else
                let isCollection = typeof<IEnumerable>.IsAssignableFrom t
                // A collection is validated through its items; properties of a model are followed, as well as items of a tuple.
                let traversable =
                    not isCollection
                    && (not(isFrameworkType t) || (FSharpType.IsTuple t && not t.IsValueType))
                // Records and unions are followed through their fields only: other members are computed and may
                // produce a new object on every call (e.g. "member this.Negated = { this with ... }").
                let fieldsOnly = traversable && (FSharpType.IsRecord(t, true) || FSharpType.IsUnion(t, true))
                let reflected = t.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
                // The properties of a model are the ones TypeDescriptor has for its type, the same as for Validator.
                let properties = [|
                    for descriptor in TypeDescriptor.GetProperties t do
                        let attributes = propertyAttributes descriptor
                        let validators = ofType<ValidationAttribute> attributes
                        // The property the descriptor stands for. There is none when a TypeDescriptionProvider adds
                        // a property of its own, and a property hidden in a derived class has no descriptor.
                        let backing =
                            reflected
                            |> Array.tryFind(fun p ->
                                p.Name = descriptor.Name
                                && p.PropertyType = descriptor.PropertyType
                                && p.GetIndexParameters().Length = 0
                                && not(isNull(p.GetGetMethod())))
                        let traverse =
                            match backing with
                            | Some p ->
                                traversable
                                && (not fieldsOnly || isField p)
                                && not(isNotObject p.PropertyType)
                                && (ofType<SkipNestedValidationAttribute> attributes).Length = 0
                                && not(isStaticLeaf visiting p.PropertyType)
                            // What a provider adds is validated as a value, the same as Validator does it.
                            | None -> false
                        if validators.Length > 0 || traverse then
                            {
                                Name = descriptor.Name
                                Get =
                                    match backing with
                                    | Some p when readsProperty descriptor -> getter t p
                                    | _ -> Getter(fun instance -> descriptor.GetValue instance)
                                Validators = validators
                                Display = Array.tryHead(ofType<DisplayAttribute> attributes) |> Option.toObj
                                Traverse = traverse
                            }
                |]
                let attributes = typeAttributes t
                let typeValidators = ofType<ValidationAttribute> attributes
                let hasItems =
                    isCollection
                    && (let items = itemTypes t
                        items.Length = 0
                        || items |> Array.exists(fun item -> not(isStaticLeaf visiting item)))

                if
                    properties.Length = 0
                    && typeValidators.Length = 0
                    && not hasItems
                    && not(typeof<IValidatableObject>.IsAssignableFrom t)
                then
                    leaf
                else
                    let composite = {
                        Kind = Composite
                        Properties = properties
                        TypeValidators = typeValidators
                        TypeDisplay = Array.tryHead(ofType<DisplayAttribute> attributes) |> Option.toObj
                        HasItems = hasItems
                        IsReferenceType = not t.IsValueType
                    }
                    let cases =
                        if properties.Length > 0 && t.IsValueType && FSharpType.IsUnion(t, true) then
                            FSharpType.GetUnionCases(t, true) |> Array.sortBy _.Tag
                        else
                            [||]
                    if cases.Length > 1 then
                        // A field belongs to every case declaring it: cases may share a field of the same name and type.
                        let fields = cases |> Array.map(fun case -> case.GetFields() |> Array.map _.Name)
                        let ofCase (own: string[]) = {
                            composite with
                                Properties =
                                    properties
                                    |> Array.filter(fun p ->
                                        Array.contains p.Name own || not(fields |> Array.exists(Array.contains p.Name)))
                        }
                        let tag = Func<obj, int>(FSharpValue.PreComputeUnionTagReader(t, true))
                        {
                            composite with
                                Kind = StructUnion(tag, Array.map ofCase fields)
                        }
                    else
                        composite

        let metadataOf (t: Type) =
            match cache.TryGetValue t with
            | true, metadata -> metadata
            | _ -> metadataFor (HashSet()) t

        let displayName (display: DisplayAttribute | null) (fallback: string) =
            match display with
            | null -> fallback
            | display ->
                match display.GetName() with
                | null
                | "" -> fallback
                | name -> name

        /// A dictionary key comparing objects by reference: unlike a comparer object, it is called directly.
        [<Struct; CustomEquality; NoComparison>]
        type Reference(target: obj) =
            member _.Target = target
            override _.GetHashCode() = RuntimeHelpers.GetHashCode target
            override _.Equals(other: obj) =
                match other with
                | :? Reference as other -> obj.ReferenceEquals(target, other.Target)
                | _ -> false
            interface IEquatable<Reference> with
                member _.Equals(other: Reference) =
                    obj.ReferenceEquals(target, other.Target)

        /// States of an object whose validation is finished. Until then its state is the order it was entered in.
        [<Literal>]
        let FoundValid = -1

        [<Literal>]
        let FoundInvalid = -2

        /// Whether there is something to validate about the object itself, apart from its members.
        let isValidatedItself (node: obj) (metadata: TypeMetadata) =
            metadata.TypeValidators.Length > 0 || node :? IValidatableObject

        /// An object waiting for the cycle it is a part of: it is validated itself when the whole cycle has been walked.
        [<Struct>]
        type Postponed = {
            Node: obj
            Metadata: TypeMetadata
            /// Path to the object, needed when it is validated itself.
            Path: string
        }

        /// Validates one object graph; results get member names relative to the root object, e.g. "Items[0].Address.Street".
        [<Sealed>]
        type Walker(rootContext: ValidationContext, results: ResizeArray<ValidationResult>) =
            /// Path to the object being validated, taken from the pool on the first descent.
            let mutable path: StringBuilder | null = null
            /// States of the reference type objects met so far, created on the first descent. Every object is validated
            /// once: it ends cycles and keeps the work linear when an object is reachable through many paths
            /// (re-validating it for each path is exponential for a chain of objects sharing their children).
            let mutable visited: Dictionary<Reference, int> | null = null
            /// How many times an invalid object was met again: its errors are not reported for the second time,
            /// but it is an invalid member of the object referring to it.
            let mutable invalidMetAgain = 0
            /// How many tracked objects have been entered: the next one gets it as its order.
            let mutable entered = 0
            /// The earliest unfinished object (its order) referred to from the object being validated, including
            /// everything nested in it. Referring to an object entered earlier means being a part of a cycle with it.
            let mutable earliest = Int32.MaxValue
            /// Objects of the cycles which have not been walked to the end yet, created for the first of them.
            let mutable postponed: ResizeArray<Postponed> | null = null
            let mutable postponedCount = 0

            /// Has to be called before visiting anything stored in the node; returns the path to append a segment to.
            member private _.Descend(node: obj, metadata: TypeMetadata) =
                if isNull visited then
                    // Nothing but the node has been entered before the first descent.
                    let tracked = Dictionary<Reference, int>()
                    if metadata.IsReferenceType then
                        tracked.Add(Reference node, 0)
                        entered <- 1
                    visited <- tracked
                match path with
                | null ->
                    let pooled = StringBuilderPool.Get()
                    path <- pooled
                    pooled
                | path -> path

            /// Gives the path back to the pool; has to be called when the walk is over, whatever its outcome.
            member _.Release() =
                match path with
                | null -> ()
                | pooled ->
                    path <- null
                    StringBuilderPool.Return pooled

            /// Returns the context of the node, creating it on the first use. Services and items come from the root context.
            member private _.ContextOf(node: obj, context: byref<ValidationContext | null>) =
                match context with
                | null ->
                    let created = ValidationContext(node, rootContext, rootContext.Items)
                    context <- created
                    created
                | context -> context

            /// Makes member names of the results added since the given index relative to the root object.
            member private this.Qualify(start: int) =
                match path with
                | null -> ()
                | path ->
                    if path.Length > 0 && results.Count > start then
                        this.Qualify(start, path.ToString())

            /// Puts the path to the validated object before the member names of the results added since the given index.
            member private _.Qualify(start: int, prefix: string) =
                for i in start .. results.Count - 1 do
                    let result = results[i]
                    let memberNames = [|
                        for name in result.MemberNames do
                            if String.IsNullOrEmpty name then
                                prefix
                            elif name[0] = '[' then
                                // An item of the object, named by the object itself.
                                prefix + name
                            else
                                prefix + "." + name
                    |]
                    results[i] <-
                        ValidationResult(
                            result.ErrorMessage,
                            (if memberNames.Length = 0 then [| prefix |] else memberNames)
                        )

            member this.Visit(value: obj) : unit =
                this.Visit(value, metadataOf(value.GetType()))

            member this.Visit(value: obj, metadata: TypeMetadata) : unit =
                match metadata.Kind with
                | Leaf -> ()
                | Optional getValue
                | KeyValue(_, getValue) -> this.VisitNullable(getValue.Invoke value)
                | ValueOptional(isSome, getValue) ->
                    if true.Equals(isSome.Invoke value) then
                        this.VisitNullable(getValue.Invoke value)
                | StructUnion(tag, cases) -> this.VisitComposite(value, cases[tag.Invoke value])
                | Composite -> this.VisitComposite(value, metadata)

            member private this.VisitNullable(value: obj | null) =
                match value with
                | null -> ()
                | value -> this.Visit value

            /// Validates the object unless it has been met before: its members and then, when they all are valid,
            /// the object itself. It is the rule of Validator, which skips IValidatableObject when an attribute
            /// of a property fails; members here include everything nested in them, validated just now or earlier.
            member private this.VisitComposite(node: obj, metadata: TypeMetadata) =
                RuntimeHelpers.EnsureSufficientExecutionStack()
                // Nothing can refer to an object which is not tracked: the order of the next tracked one is enough for it.
                let order = entered
                let isMetAgain =
                    match visited with
                    // The outermost object: it becomes the first tracked one when something nested in it is found.
                    | null -> false
                    | visited ->
                        if not metadata.IsReferenceType then
                            false
                        elif visited.TryAdd(Reference node, order) then
                            entered <- order + 1
                            false
                        else
                            let state = visited[Reference node]
                            if state = FoundInvalid then
                                invalidMetAgain <- invalidMetAgain + 1
                            elif state >= 0 && state < earliest then
                                // Not finished, which is a cycle: it contains the object referring to it.
                                earliest <- state
                            true
                if not isMetAgain then
                    let errorsBefore = results.Count
                    let metAgainBefore = invalidMetAgain
                    let outerEarliest = earliest
                    let postponedBefore = postponedCount
                    earliest <- Int32.MaxValue
                    let context = this.ValidateMembers(node, metadata)
                    if earliest < order then
                        // Something in the object refers to an object entered earlier and not finished. That one contains
                        // this object, so they are members of each other, and whether the members are valid is known
                        // when that object has been walked to the end.
                        this.Postpone(node, metadata)
                        if outerEarliest < earliest then
                            earliest <- outerEarliest
                    else
                        earliest <- outerEarliest
                        if postponedCount > postponedBefore then
                            this.ValidatePostponed(postponedBefore, errorsBefore, metAgainBefore)
                        let mutable valid = results.Count = errorsBefore && invalidMetAgain = metAgainBefore
                        if valid && isValidatedItself node metadata then
                            this.ValidateItself(node, metadata, context)
                            this.Qualify errorsBefore
                            valid <- results.Count = errorsBefore
                        let state = if valid then FoundValid else FoundInvalid
                        if postponedCount > postponedBefore then
                            this.Finish(postponedBefore, state)
                        if metadata.IsReferenceType then
                            match visited with
                            | null -> ()
                            | visited -> visited[Reference node] <- state

            /// Leaves the object for the time when the cycle it is a part of has been walked.
            member private _.Postpone(node: obj, metadata: TypeMetadata) =
                let waiting =
                    match postponed with
                    | null ->
                        let created = ResizeArray()
                        postponed <- created
                        created
                    | waiting -> waiting
                waiting.Add {
                    Node = node
                    Metadata = metadata
                    Path =
                        match path with
                        | null -> ""
                        | path ->
                            if isValidatedItself node metadata then
                                path.ToString()
                            else
                                ""
                }
                postponedCount <- waiting.Count

            /// Validates the objects which have been waiting for the current one, the first object of their cycle.
            /// They all contain each other, so each of them is validated itself only when nothing wrong has been found
            /// since the cycle was entered. The order is the same as without a cycle: nested objects come first.
            member private this.ValidatePostponed(first: int, errorsBefore: int, metAgainBefore: int) =
                match postponed with
                | null -> ()
                | postponed ->
                    let mutable index = first
                    while index < postponed.Count
                          && results.Count = errorsBefore
                          && invalidMetAgain = metAgainBefore do
                        let waiting = postponed[index]
                        if isValidatedItself waiting.Node waiting.Metadata then
                            this.ValidateItself(waiting.Node, waiting.Metadata, null)
                            if waiting.Path.Length > 0 && results.Count > errorsBefore then
                                this.Qualify(errorsBefore, waiting.Path)
                        index <- index + 1

            /// Gives the objects of a walked cycle their state: containing each other, they are valid or invalid together.
            member private _.Finish(first: int, state: int) =
                match postponed with
                | null -> ()
                | postponed ->
                    match visited with
                    | null -> ()
                    | visited ->
                        for index in first .. postponed.Count - 1 do
                            let waiting = postponed[index]
                            if waiting.Metadata.IsReferenceType then
                                visited[Reference waiting.Node] <- state
                    postponed.RemoveRange(first, postponed.Count - first)
                    postponedCount <- first

            /// Validates the properties of the object and everything stored in it;
            /// returns the context of the object when one was needed.
            member private this.ValidateMembers(node: obj, metadata: TypeMetadata) : ValidationContext | null =
                // One context serves all members of the node, unlike Validator which creates one per property.
                let mutable nodeContext: ValidationContext | null = null

                for property in metadata.Properties do
                    let value = property.Get.Invoke node
                    if property.Validators.Length > 0 then
                        let context = this.ContextOf(node, &nodeContext)
                        context.MemberName <- property.Name
                        context.DisplayName <- displayName property.Display property.Name
                        let start = results.Count
                        if not(Validator.TryValidateValue(value, context, results, property.Validators)) then
                            this.Qualify start
                    if property.Traverse then
                        match value with
                        | null -> ()
                        | value ->
                            let valueMetadata = metadataOf(value.GetType())
                            if not valueMetadata.Kind.IsLeaf then
                                let path = this.Descend(node, metadata)
                                let length = path.Length
                                if length > 0 then
                                    path.Append('.') |> ignore
                                path.Append(property.Name) |> ignore
                                this.Visit(value, valueMetadata)
                                path.Length <- length

                if metadata.HasItems then
                    let mutable index = 0
                    for item in node :?> IEnumerable do
                        if not(isNull item) then
                            let itemMetadata = metadataOf(item.GetType())
                            match itemMetadata.Kind with
                            | Leaf -> ()
                            | KeyValue(getKey, getValue) ->
                                match getValue.Invoke item with
                                | null -> ()
                                | value ->
                                    let valueMetadata = metadataOf(value.GetType())
                                    if not valueMetadata.Kind.IsLeaf then
                                        let path = this.Descend(node, metadata)
                                        let length = path.Length
                                        path.Append('[').Append(string(getKey.Invoke item)).Append(']') |> ignore
                                        this.Visit(value, valueMetadata)
                                        path.Length <- length
                            | _ ->
                                let path = this.Descend(node, metadata)
                                let length = path.Length
                                path.Append('[').Append(index).Append(']') |> ignore
                                this.Visit(item, itemMetadata)
                                path.Length <- length
                        index <- index + 1

                nodeContext

            /// Validates the object itself: the attributes applied to its type and then IValidatableObject.
            member private _.ValidateItself(node: obj, metadata: TypeMetadata, nodeContext: ValidationContext | null) =
                let start = results.Count
                let context =
                    if obj.ReferenceEquals(node, rootContext.ObjectInstance) then
                        rootContext
                    else
                        let context =
                            match nodeContext with
                            | null -> ValidationContext(node, rootContext, rootContext.Items)
                            | context -> context
                        context.MemberName <- null
                        context.DisplayName <- displayName metadata.TypeDisplay (node.GetType().Name)
                        context
                if metadata.TypeValidators.Length > 0 then
                    Validator.TryValidateValue(node, context, results, metadata.TypeValidators)
                    |> ignore
                if results.Count = start then
                    match node with
                    | :? IValidatableObject as validatable ->
                        let objectResults = validatable.Validate context
                        if not(isNull(box objectResults)) then
                            for result in objectResults do
                                if not(isNull(box result)) then
                                    results.Add result
                    | _ -> ()

    /// <summary>
    /// Manually validate an object of type 'T` with a provided <see cref="System.ComponentModel.DataAnnotations.ValidationContext"/>.
    /// Validation is recursive: nested objects, collection items, dictionary values and option values are validated as well,
    /// their errors get member names like <c>Address.Street</c>, <c>Items[0].Name</c> or <c>Translations[en].Title</c>.
    /// <param name="ctx">The validation context object, created for the model.</param>
    /// <param name="model">The model object to validate.</param>
    /// </summary>
    let validateModelWith (ctx: ValidationContext) (model: 'T) =
        let instance = ctx.ObjectInstance
        let boxed = box model
        // A value type model is boxed anew wherever an object is needed, so it is never the instance of the context:
        // the two have to be equal, and it is the model which is validated.
        let isInstance =
            if typeof<'T>.IsValueType then
                instance.Equals boxed
            else
                obj.ReferenceEquals(instance, boxed)
        if not isInstance then
            invalidArg (nameof ctx) "The model must be the ObjectInstance of the ValidationContext supplied."
        let rootContext =
            if typeof<'T>.IsValueType then
                // The context of the box which is validated: it has everything the supplied one does.
                let context = ValidationContext(nonNull boxed, ctx, ctx.Items)
                context.MemberName <- ctx.MemberName
                context.DisplayName <- ctx.DisplayName
                context
            else
                ctx
        let validationResults = ResizeArray()
        let walker = ObjectGraph.Walker(rootContext, validationResults)
        try
            walker.Visit rootContext.ObjectInstance
        finally
            walker.Release()
        if validationResults.Count = 0 then
            ModelValidationResult.Valid model
        else
            ModelValidationResult.Invalid(model, ValidationErrors(validationResults))

    /// <summary>
    /// Manually validate an object of type 'T`.
    /// Validation is recursive: nested objects, collection items, dictionary values and option values are validated as well,
    /// their errors get member names like <c>Address.Street</c>, <c>Items[0].Name</c> or <c>Translations[en].Title</c>.
    /// </summary>
    let validateModel (model: 'T) =
        validateModelWith (ValidationContext(model)) model

    type HttpContextExtensions =

        /// <summary>
        /// Uses the <see cref="Serializers.IJsonSerializer"/> to deserialize the entire body of the <see cref="Microsoft.AspNetCore.Http.HttpRequest"/> asynchronously into an object of type 'T and then validate it.
        /// </summary>
        /// <typeparam name="'T"></typeparam>
        /// <returns>Returns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
        [<Extension>]
        static member BindAndValidateJson<'T when 'T: not null>(ctx: HttpContext) =
            task {
                let! result = ctx.BindJson<'T>()
                let validationContext = ValidationContext(result, ctx.RequestServices, ctx.Items)
                return validateModelWith validationContext result
            }

        /// <summary>
        /// Parses all input elements from an HTML form into an object of type 'T and then validates it.
        /// </summary>
        /// <param name="ctx">The current http context object.</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>Returns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
        [<Extension>]
        static member BindAndValidateForm<'T when 'T: not null>(ctx: HttpContext) =
            task {
                let! result = ctx.BindForm<'T>()
                let validationContext = ValidationContext(result, ctx.RequestServices, ctx.Items)
                return validateModelWith validationContext result
            }

        /// <summary>
        /// Parses all parameters of a request's query string into an object of type 'T and then validates it.
        /// </summary>
        /// <param name="ctx">The current http context object.</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>Returns an instance of type 'T</returns>
        [<Extension>]
        static member BindAndValidateQuery<'T when 'T: not null>(ctx: HttpContext) =
            let result = ctx.BindQuery<'T>()
            let validationContext = ValidationContext(result, ctx.RequestServices, ctx.Items)
            validateModelWith validationContext result

    [<AutoOpen>]
    module RequestHandlers =
        /// <summary>
        /// Parses a JSON payload into an instance of type 'T and validates it.
        /// </summary>
        /// <param name="f">A function which accepts an object of type ValidationResult<'T> and returns a <see cref="EndpointHandler"/> function.</param>
        /// <param name="ctx">HttpContext</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
        let bindAndValidateJson<'T when 'T: not null>
            (f: ModelValidationResult<'T> -> EndpointHandler)
            : EndpointHandler =
            fun (ctx: HttpContext) ->
                task {
                    let! model = ctx.BindJson<'T>()
                    let validationContext = ValidationContext(model, ctx.RequestServices, ctx.Items)
                    return! f (validateModelWith validationContext model) ctx
                }

        /// <summary>
        /// Parses a HTTP form payload into an instance of type 'T and validates it.
        /// </summary>
        /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
        /// <param name="ctx">HttpContext</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
        let bindAndValidateForm<'T when 'T: not null>
            (f: ModelValidationResult<'T> -> EndpointHandler)
            : EndpointHandler =
            fun (ctx: HttpContext) ->
                task {
                    let! model = ctx.BindForm<'T>()
                    let validationContext = ValidationContext(model, ctx.RequestServices, ctx.Items)
                    return! f (validateModelWith validationContext model) ctx
                }

        /// <summary>
        /// Parses a HTTP query string into an instance of type 'T and validates it.
        /// </summary>
        /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
        /// <param name="ctx">HttpContext</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
        let bindAndValidateQuery<'T when 'T: not null>
            (f: ModelValidationResult<'T> -> EndpointHandler)
            : EndpointHandler =
            fun (ctx: HttpContext) ->
                let model = ctx.BindQuery<'T>()
                let validationContext = ValidationContext(model, ctx.RequestServices, ctx.Items)
                f (validateModelWith validationContext model) ctx
