namespace Oxpecker

[<AutoOpen>]
module ModelValidation =

    open System
    open System.Collections
    open System.Collections.Concurrent
    open System.Collections.Generic
    open System.ComponentModel.DataAnnotations
    open System.Linq.Expressions
    open System.Reflection
    open System.Runtime.CompilerServices
    open System.Text
    open Microsoft.AspNetCore.Http
    open Microsoft.FSharp.Reflection

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
            /// An object with properties, type-level validation and/or items to validate.
            | Composite

        type TypeMetadata = {
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

        /// Whether the property is a field of a record or of a union case.
        let isField (p: PropertyInfo) =
            match p.GetCustomAttribute<CompilationMappingAttribute>() with
            | null -> false
            | mapping -> mapping.SourceConstructFlags = SourceConstructFlags.Field

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

        let rec metadataFor (visiting: HashSet<Type>) (t: Type) =
            match cache.TryGetValue t with
            | true, metadata -> metadata
            | _ ->
                visiting.Add t |> ignore
                let metadata = createMetadata visiting t
                visiting.Remove t |> ignore
                cache[t] <- metadata
                metadata

        /// Whether a value of the declared type can never contain anything to validate. Only sealed types are decided
        /// statically, the rest depends on the runtime type. A type which is still being analysed (a recursive one)
        /// is not a leaf.
        and isStaticLeaf (visiting: HashSet<Type>) (t: Type) =
            t.IsSealed && not(visiting.Contains t) && (metadataFor visiting t).Kind.IsLeaf

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
                let properties = [|
                    for p in t.GetProperties(BindingFlags.Public ||| BindingFlags.Instance) do
                        if p.GetIndexParameters().Length = 0 && not(isNull(p.GetGetMethod())) then
                            let validators = Seq.toArray(p.GetCustomAttributes<ValidationAttribute>(true))
                            let propertyType = p.PropertyType
                            let traverse =
                                traversable
                                && (not fieldsOnly || isField p)
                                && not(isNotObject propertyType)
                                && not(Attribute.IsDefined(p, typeof<SkipNestedValidationAttribute>, true))
                                && not(isStaticLeaf visiting propertyType)
                            if validators.Length > 0 || traverse then
                                {
                                    Name = p.Name
                                    Get = getter t p
                                    Validators = validators
                                    Display = p.GetCustomAttribute<DisplayAttribute>(true)
                                    Traverse = traverse
                                }
                |]
                let typeValidators = Seq.toArray(t.GetCustomAttributes<ValidationAttribute>(true))
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
                    {
                        Kind = Composite
                        Properties = properties
                        TypeValidators = typeValidators
                        TypeDisplay = t.GetCustomAttribute<DisplayAttribute>(true)
                        HasItems = hasItems
                        IsReferenceType = not t.IsValueType
                    }

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

        /// Validates one object graph; results get member names relative to the root object, e.g. "Items[0].Address.Street".
        [<Sealed>]
        type Walker(rootContext: ValidationContext, results: ResizeArray<ValidationResult>) =
            /// Path to the object being validated, created on the first descent.
            let mutable path: StringBuilder | null = null
            /// Reference type objects validated so far, created on the first descent. Every object is validated once:
            /// it ends cycles and keeps the work linear when an object is reachable through many paths
            /// (re-validating it for each path is exponential for a chain of objects sharing their children).
            /// The value is the number of problems found by the time the object was entered.
            let mutable visited: Dictionary<obj, int> | null = null
            /// How many times an invalid object was met again: its errors are not reported for the second time,
            /// but it is an invalid member of the object referring to it.
            let mutable invalidMetAgain = 0

            /// Returns false when the object has already been validated, or is being validated further up the path.
            member private _.Enter(node: obj) =
                match visited with
                | null -> true
                | visited ->
                    let problems = results.Count + invalidMetAgain
                    if visited.TryAdd(node, problems) then
                        true
                    else
                        // Has anything gone wrong since the object was entered? While the object is still being validated
                        // (a cycle), it went wrong inside of it. Once the object is finished, it may also have gone wrong
                        // elsewhere, but telling that apart takes a second write for every object, so the object is
                        // treated as invalid. That is safe: the model is invalid anyway, and the object referring to
                        // this one is validated itself as soon as the problem is fixed.
                        if problems > visited[node] then
                            invalidMetAgain <- invalidMetAgain + 1
                        false

            /// Has to be called before visiting anything stored in the node; returns the path to append a segment to.
            member private _.Descend(node: obj, metadata: TypeMetadata, errorsBefore: int) =
                if isNull visited then
                    // Nothing but the node has been entered before the first descent.
                    let tracked = Dictionary<obj, int>(ReferenceEqualityComparer.Instance)
                    if metadata.IsReferenceType then
                        tracked.Add(node, errorsBefore)
                    visited <- tracked
                match path with
                | null ->
                    let created = StringBuilder()
                    path <- created
                    created
                | path -> path

            /// Returns the context of the node, creating it on the first use. Services and items come from the root context.
            member private _.ContextOf(node: obj, context: byref<ValidationContext | null>) =
                match context with
                | null ->
                    let created = ValidationContext(node, rootContext, rootContext.Items)
                    context <- created
                    created
                | context -> context

            /// Makes member names of the results added since the given index relative to the root object.
            member private _.Qualify(start: int) =
                match path with
                | null -> ()
                | path ->
                    if path.Length > 0 && results.Count > start then
                        let prefix = path.ToString()
                        for i in start .. results.Count - 1 do
                            let result = results[i]
                            let memberNames = [|
                                for name in result.MemberNames do
                                    if String.IsNullOrEmpty name then
                                        prefix
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
                | Composite -> this.VisitComposite(value, metadata)

            member private this.VisitNullable(value: obj | null) =
                match value with
                | null -> ()
                | value -> this.Visit value

            member private this.VisitComposite(node: obj, metadata: TypeMetadata) =
                RuntimeHelpers.EnsureSufficientExecutionStack()
                if not metadata.IsReferenceType || this.Enter node then
                    let errorsBefore = results.Count
                    let metAgainBefore = invalidMetAgain
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
                                    let path = this.Descend(node, metadata, errorsBefore)
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
                                            let path = this.Descend(node, metadata, errorsBefore)
                                            let length = path.Length
                                            path.Append('[').Append(string(getKey.Invoke item)).Append(']') |> ignore
                                            this.Visit(value, valueMetadata)
                                            path.Length <- length
                                | _ ->
                                    let path = this.Descend(node, metadata, errorsBefore)
                                    let length = path.Length
                                    path.Append('[').Append(index).Append(']') |> ignore
                                    this.Visit(item, itemMetadata)
                                    path.Length <- length
                            index <- index + 1

                    // Same as in Validator: the object itself is validated only when its members are valid,
                    // and members here include everything nested in them, validated just now or earlier.
                    if
                        results.Count = errorsBefore
                        && invalidMetAgain = metAgainBefore
                        && (metadata.TypeValidators.Length > 0 || node :? IValidatableObject)
                    then
                        let context =
                            if obj.ReferenceEquals(node, rootContext.ObjectInstance) then
                                rootContext
                            else
                                let context = this.ContextOf(node, &nodeContext)
                                context.MemberName <- null
                                context.DisplayName <- displayName metadata.TypeDisplay (node.GetType().Name)
                                context
                        if metadata.TypeValidators.Length > 0 then
                            Validator.TryValidateValue(node, context, results, metadata.TypeValidators)
                            |> ignore
                        if results.Count = errorsBefore then
                            match node with
                            | :? IValidatableObject as validatable ->
                                let objectResults = validatable.Validate context
                                if not(isNull(box objectResults)) then
                                    for result in objectResults do
                                        if not(isNull(box result)) then
                                            results.Add result
                            | _ -> ()
                        this.Qualify errorsBefore

    /// <summary>
    /// Manually validate an object of type 'T` with a provided <see cref="System.ComponentModel.DataAnnotations.ValidationContext"/>.
    /// Validation is recursive: nested objects, collection items, dictionary values and option values are validated as well,
    /// their errors get member names like <c>Address.Street</c>, <c>Items[0].Name</c> or <c>Translations[en].Title</c>.
    /// <param name="ctx">The validation context object, created for the model.</param>
    /// <param name="model">The model object to validate.</param>
    /// </summary>
    let validateModelWith (ctx: ValidationContext) (model: 'T) =
        let instance = ctx.ObjectInstance
        // A value type model is boxed anew wherever an object is needed: it is the same model when the values are equal.
        let isInstance =
            if typeof<'T>.IsValueType then
                instance.Equals(box model)
            else
                obj.ReferenceEquals(instance, model)
        if not isInstance then
            invalidArg (nameof ctx) "The model must be the ObjectInstance of the ValidationContext supplied."
        let validationResults = ResizeArray()
        ObjectGraph.Walker(ctx, validationResults).Visit instance
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
