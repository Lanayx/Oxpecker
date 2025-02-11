namespace Oxpecker.Solid

open System
open Fable
open Fable.AST
open Fable.AST.Fable

[<assembly: ScanForPlugins>]
do () // Prompts fable to utilise this plugin

type Tracer = Tracer of string seq with
    static member create s = Tracer s
    static member init = Tracer (seq {})
    member inline this.Value = let (Tracer value) = this in value
    #if DEBUG
    member this.add s = Tracer (this.Value |> Seq.append s)
    member this.add (t : Tracer) = Tracer (this.Value |> Seq.append t.Value)
    #else
    member this.add 't = this
    #endif

module internal rec AST =
    /// <summary>
    /// Property definition of a <c>string * Fable.Expr</c> tuple
    /// </summary>
    type Property = string * Expr
    module Property = let create s e  : Property = s, e
    /// <summary>
    /// List of Property definitions
    /// </summary>
    type PropertyList = Property list
    /// <summary>A tags name or import origin expression</summary>
    type TagSource =
        | AutoImport of name: string
        | LibraryImport of importExpr: Expr
    /// <summary>
    /// The possible information groupings that are contained in an elements Builder
    /// </summary>
    type TagBuilderInfo =
        | Children of propsAndChildren: CallInfo
        | Props of props: Expr list
        | Combined  of props: Expr list * propsAndChildren: CallInfo
        static member create c = Children c
        static member create p = Props p
        static member create (e: Expr list, c: CallInfo) = Combined (e,c)
    /// <summary>
    /// Collation of an elements build information
    /// </summary>
    type TagBuilder =
        {
            Tag: TagSource
            Info: TagBuilderInfo
            Range: SourceLocation option
            Tracer: Tracer
        }
        static member create (tag, info, range) = { Tag=tag;Info=info;Range=range;Tracer=Tracer.init}
        static member create (tag, info) = { Tag=tag;Info=info;Range=None;Tracer=Tracer.init}
        static member tag tag builder = { builder with Tag = tag }
        static member info info builder = { builder with Info = info }
        static member range range (builder : TagBuilder) = { builder with Range = range }
    /// <summary>
    /// A special handled property/attribute pair, produced by Element Extensions such as attr, bool etc
    /// </summary>
    type EventHandler = { Name : string ; Handler : Fable.Expr }
    module TagBuilder =
        type info =
            /// <summary>
            /// Adds an expression to a builders information collection
            /// </summary>
            /// <param name="e"><c>Fable.Expr</c></param>
            /// <param name="builder"><c>TagBuilder</c></param>
            /// <returns>If the builder has a prop list, then it will append the expression to the list</returns>
            static member add e builder =
                match builder.Info with
                | Props propertyList
                | Combined (propertyList, _) -> // TODO -> should this keep subtype?
                    { builder with Info = TagBuilderInfo.create (e :: propertyList) }
                | _ ->
                    failwith "Impossible"
            /// <summary>
            /// Adds an expression list to a builders information collection
            /// </summary>
            /// <param name="es"><c>Fable.Expr list</c></param>
            /// <param name="builder"><c>TagBuilder</c></param>
            /// <returns>Adds the expression list to the already existing expression list, or changes the type
            /// of information collection to one that takes expression list (property type)</returns>
            static member adds es builder =
                match builder.Info with
                | Children propsAndChildren -> { builder with Info = TagBuilderInfo.create (es, propsAndChildren) }
                | Props props -> { builder with Info = TagBuilderInfo.create (es @ props) }
                | Combined(props, propsAndChildren) -> { builder with Info = TagBuilderInfo.create (es @ props, propsAndChildren) }
        /// <summary>
        /// Collates and builds the collected tag properties and children
        /// </summary>
        /// <param name="builder"><c>TagBuilder</c></param>
        /// <returns>Flattened expression of the built Tag</returns>
        let transform builder =
            let properties, children =
                match builder.Info with
                | Children propsAndChildren ->
                    (propsAndChildren.Args |> List.fold Attributes.get [],
                     propsAndChildren.Args |> List.fold Children.get [])
                | Props props ->
                    (Attributes.collect props,
                     [])
                | Combined(props, propsAndChildren) ->
                    (Attributes.collect props,
                     propsAndChildren.Args |> List.fold Children.get [])
            let transformedProperties =
                properties |> List.map (fun (name, expr) ->
                    Value(
                        kind = NewTuple(
                            values = [
                                Value(kind = StringConstant name, range = None)
                                TypeCast(expr, Type.Any)
                            ],
                            isStruct = false
                        ),
                        range = None
                    ))
            let childrenPropertyCollection =
                (children |> Helpers.convertListToExpr |> Children.wrap ) :: transformedProperties
                |> Helpers.convertListToExpr
            let tag =
                match builder.Tag with
                | AutoImport tagName -> Value(StringConstant tagName, None)
                | LibraryImport import -> import

            Call(
                callee = Baked.importJsxCreate,
                info = {
                    ThisArg = None
                    Args = [ tag; childrenPropertyCollection ]
                    SignatureArgTypes = []
                    GenericArgs = []
                    MemberRef = None
                    Tags = [ "jsx" ]
                },
                typ = Baked.jsxElementType,
                range = builder.Range
                )

    // Pattern matchers //
    (*
    The pattern matchers collate under the root AST type.
    ie - Let roots will have their active patterns collected under the `Let` module
    eg - Let.WithChildren
    *)

    [<RequireQualifiedAccess>]
    module Ident =
        let (|Equals|_|) (value : string) : Ident -> unit option =
            function
            | { Name = name } when name = value -> Some()
            | _ -> None
        let (|StartsWith|_|) (value : string) : Ident -> unit option =
            function
            | { Name = name } when name.StartsWith value -> Some()
            | _ -> None
    [<RequireQualifiedAccess>]
    module Call =
        let (|TagImport|_|) expr =
            match expr with
            | Call(Import({ Kind = UserImport false }, Any, _) as import, { Tags = [ "new" ] }, DeclaredType(typ, []), range)
                when typ.FullName.StartsWith "Oxpecker.Solid" ->
                Some(import, range)
            | _ -> None
        let (|Tag|_|) condition = function
            | Call(Import(importInfo, LambdaType(_, DeclaredType(typ, _)), _), callInfo, _, range) when condition importInfo ->
                let getTagSource = function
                    | Call.TagImport(imp, _) :: _
                    | Let(_, Call.TagImport(imp, _), _) :: _ -> LibraryImport imp
                    | _ ->
                        let tagName = typ.FullName.Split('.') |> Seq.last
                        match tagName with
                        | "Fragment" -> ""
                        | _ when tagName.EndsWith("'") -> tagName[0..tagName.Length-1]
                        | _ when tagName.EndsWith("`1") -> tagName[0..tagName.Length-2]
                        | _ -> tagName
                        |> AutoImport
                callInfo
                |> _.Args
                |> getTagSource
                |> fun tagSource ->
                    TagBuilder.create (tagSource, TagBuilderInfo.create callInfo, range)
                    |> Some
            | _ -> None
        let (|ImportCallRangeInfo|_|) = function
            | Call(Import(importInfo, _, _), callInfo, _, range) -> Some (importInfo, callInfo, range)
            | _ -> None
    [<RequireQualifiedAccess>]
    module Tag =
        let (|NoChildren|_|) expr =
            let condition = _.Selector.EndsWith("_$ctor")
            match expr with
            | Call.Tag condition tagBuilder -> Some tagBuilder
            | _ -> None
        let (|WithChildren|_|) expr =
            let condition =
                fun importInfo ->
                    importInfo.Selector.StartsWith("HtmlContainerExtensions_Run")
                    || importInfo.Selector.StartsWith("BindingsModule_Extensions_Run")
            match expr with
            | Call.Tag condition tagBuilder -> Some tagBuilder
            | _ -> None
        let (|NoChildrenWithHandler|_|) expr =
            match expr with
            | Call.ImportCallRangeInfo (importInfo, { Args = args :: _ }, range)
                when importInfo.Selector.StartsWith("HtmlElementExtensions_") ->
                match args with
                | Tag.NoChildren builder ->
                    builder
                    |> TagBuilder.info (TagBuilderInfo.create [ expr ])
                    |> TagBuilder.range range
                    |> Some
                | Let.NoChildrenWithProps builder
                | Tag.NoChildrenWithHandler builder ->
                    builder
                    |> TagBuilder.info.add expr
                    |> TagBuilder.range range
                    |> Some
                | _ -> None
            | _ -> None
    [<RequireQualifiedAccess>]
    module Let =
        let (|Element|_|) = function
            | Let(Ident.StartsWith "element", _, _) -> Some()
            | _ -> None
        let (|WithChildren|_|) = function
            | Let(_, Tag.WithChildren builder, _) -> builder |> Some
            | _ -> None
        let (|NoChildrenWithProps|_|) = function
            | Let(_, Tag.NoChildren builder, Sequential exprs) -> Some { builder with Info = TagBuilderInfo.create exprs }
            | _ -> None
        let (|NoChildrenNoProps|_|) =
            function
            | Let(_, Tag.NoChildren builder, _) ->
                builder
                |> TagBuilder.info (TagBuilderInfo.create [])
                |> Some
            | _ -> None
    [<RequireQualifiedAccess>]
    module Lambda =
        let (|TextNoSiblings|_|) = function
            | Lambda(Ident.StartsWith "cont", TypeCast(textBody, Unit), None) ->
                Some textBody
            | _ -> None
    [<RequireQualifiedAccess>]
    module CallInfo =
        let (|EventHandler|_|) callInfo =
            match callInfo with
            | { Args = [ _; Value(StringConstant eventName, _); handler ] } ->
                Some({ Name = eventName ; Handler = handler })
            | _ -> None
    [<RequireQualifiedAccess>]
    module Baked =
        let jsxElementType =
            Type.DeclaredType(
                ref = {
                    FullName = "Fable.Core.JSX.Element"
                    Path = EntityPath.CoreAssemblyName "Fable.Core"
                },
                genericArgs = []
            )
        let importJsxCreate =
            Import(
                info = {
                    Selector = "create"
                    Path = "@fable-org/fable-library-js/JSX.js"
                    Kind = ImportKind.UserImport false
                },
                typ = Type.Any,
                range = None
            )
        let listItemType =
            Type.Tuple(genericArgs = [ Type.String; Type.Any ], isStruct = false)
        let emptyList =
            Value(kind = NewList(headAndTail = None, typ = listItemType), range = None)

    [<RequireQualifiedAccess>]
    module Attributes =
        /// <summary>
        /// Matches predefined Element Extension attributes/properties when given
        /// a <c>string * CallInfo</c> tuple
        /// </summary>
        let (|ElementExtension|_|) = function
            | "on", CallInfo.EventHandler eventHandler -> Property.create ("on:" + eventHandler.Name) eventHandler.Handler |> Some
            | "bool", CallInfo.EventHandler eventHandler -> Property.create ("bool:" + eventHandler.Name) eventHandler.Handler |> Some
            | "data", CallInfo.EventHandler eventHandler -> Property.create ("data-" + eventHandler.Name) eventHandler.Handler |> Some
            | "attr", CallInfo.EventHandler eventHandler -> Property.create eventHandler.Name eventHandler.Handler |> Some
            | "ref", { Args = [ _; identExpr ] } -> Property.create "ref" identExpr |> Some
            | "style", { Args = [ _; identExpr ] } -> Property.create "style" identExpr |> Some
            | "classList", { Args = [ _; identExpr ] } -> Property.create "classList" identExpr |> Some
            | _ -> None
        let rec collect exprs =
            match exprs with
            | [] -> []
            | Sequential expressions :: rest -> Attributes.collect rest @ Attributes.collect expressions
            | Call.ImportCallRangeInfo (importInfo, callInfo, _) :: rest ->
                let restResults = Attributes.collect rest
                match importInfo.Kind with
                | ImportKind.MemberImport(MemberRef(entity, memberRefInfo))
                    when entity.FullName.StartsWith("Oxpecker.Solid") ->
                    match memberRefInfo.CompiledName, callInfo with
                    | Attributes.ElementExtension extensionProperty -> extensionProperty :: restResults
                    | _ ->
                        let setterIndex = memberRefInfo.CompiledName.IndexOf("set_")
                        if setterIndex >= 0 then
                            let propertyName =
                                match memberRefInfo.CompiledName.Substring(setterIndex + "set_".Length) with
                                | name when name.EndsWith("'") -> name[0..name.Length - 1]
                                | name when name.StartsWith("aria") -> $"aria-{name.Substring(4).ToLower()}"
                                | name -> name
                            let propertyValue = callInfo.Args.Head
                            match propertyValue with
                            | TypeCast(expr, DeclaredType({ FullName = "Oxpecker.Solid.Builder.HtmlElement" }, _)) ->
                                transform expr
                                |> Property.create propertyName
                            | Delegate(args, expr, name, tags) ->
                                Delegate(args, transform expr, name, tags)
                                |> Property.create propertyName
                            | _ -> Property.create propertyName propertyValue
                            |> fun property -> property :: restResults
                        else
                            restResults
                | _ -> restResults
            | Set(IdentExpr(Ident.StartsWith "returnVal"), SetKind.FieldSet name, _, handler, _) :: rest ->
                let propertyName = name |> function
                    | _ when name.EndsWith("'") -> name.Substring(0, name.Length - 1)
                    | _ -> name
                Property.create propertyName handler
                |> fun property -> property :: Attributes.collect rest
            | _ :: rest -> Attributes.collect rest
        let get current expr : PropertyList =
            match expr with
            | Let(Ident.StartsWith "returnVal", _, Sequential exprs) ->
                Attributes.collect exprs @ current
            | Tag.NoChildrenWithHandler { Info = TagBuilderInfo.Props props } ->
                Attributes.collect props @ current
            | _ -> current
    module Children =
        let get current expr : Expr list =
            match expr with
            | Let.WithChildren builder
            | Let.Element & Let(_, Let.NoChildrenWithProps builder, _)
            | Let.Element & Let.NoChildrenNoProps builder
            | Let.Element & Let(_, Tag.NoChildrenWithHandler builder, _) ->
                TagBuilder.transform builder :: current
            | Let.Element & Let(_, next, _) ->
                transform next :: current
            | Lambda(Ident.StartsWith "cont", TypeCast(Lambda(item, Lambda(index, next, _), _), _), _) ->
                Delegate([ item; index ], transform next, None, []) :: current
            | Lambda.TextNoSiblings body -> body :: current
            | Let(Ident.StartsWith "text", body, Lambda.TextNoSiblings _) ->
                body :: current
            | Let(Ident.StartsWith "second", next, Lambda(Ident.StartsWith "builder", Sequential(TypeCast(textBody, Unit) :: _), _)) ->
                Children.get (textBody :: current) next
            | CurriedApply(Lambda(Ident.StartsWith "cont", TypeCast(lastParameter, Unit), Some second), _, _, _) when second.StartsWith "second" ->
                lastParameter :: current
            | CurriedApply(Lambda(Ident.StartsWith "builder", Sequential [ TypeCast(middleParameter, Unit); next ], _), _, _, _)
            | Lambda(Ident.StartsWith "builder", Sequential [ TypeCast(middleParameter, Unit); next ], _) ->
                Children.get (middleParameter :: current) next
            | Let(Ident.StartsWith "first", next1, Lambda(Ident.StartsWith "builder", Sequential [ CurriedApply _; next2 ], _)) ->
                let next2Children = Children.get [] next2
                let next1Children = Children.get [] next1
                next2Children @ next1Children @ current
            | Let(Ident.StartsWith "first", Let(_, expr, _), Let(Ident.StartsWith "second", next, _))
            | Let(Ident.StartsWith "first", expr, Let(Ident.StartsWith "second", next, _)) ->
                match expr with
                | Let.NoChildrenNoProps builder ->
                    Children.get (TagBuilder.transform builder :: current) next
                | Tag.NoChildren builder ->
                    TagBuilder.transform (builder |> TagBuilder.info (TagBuilderInfo.create []))
                    |> fun newExpr -> Children.get (newExpr :: current) next
                | Tag.WithChildren builder ->
                    TagBuilder.transform builder
                    |> fun newExpr -> Children.get (newExpr :: current) next
                | _ ->
                    Children.get (transform expr :: current) next
            | IfThenElse(guardExpr, thenExpr, elseExpr, range) ->
                IfThenElse(guardExpr, transform thenExpr, transform elseExpr, range) :: current
            | DecisionTree(decisionTree, targets) ->
                DecisionTree(decisionTree, targets |> List.map(fun (target,expr) -> target, transform expr)) :: current
            | Call(Get(IdentExpr _, FieldGet _, Any, _), { Args = args }, _, _) ->
                match args with
                | [ Call(Import({Selector = "uncurry2"}, Any, None), { Args = [ Lambda(_, body, _) ] }, _, _) ] ->
                    Children.get current body
                | [ Call.TagImport (imp, _) ] ->
                    TagBuilder.transform (TagBuilder.create (LibraryImport imp, TagBuilderInfo.create []))
                    |> fun newExpr -> newExpr :: current
                | [ Let(_, Call.TagImport(imp, _), Sequential exprs) ] ->
                    TagBuilder.transform (TagBuilder.create (LibraryImport imp, TagBuilderInfo.create exprs))
                    |> fun newExpr -> newExpr :: current
                | [ Let(_, Let(_, Call.TagImport(imp, _), Sequential exprs), Tag.WithChildren { Info = TagBuilderInfo.Children callInfo }) ] ->
                    TagBuilder.create ((LibraryImport imp), (TagBuilderInfo.create(exprs, callInfo)))
                    |> TagBuilder.transform
                    |> fun newExpr -> newExpr :: current
                | [ next1; next2 ] ->
                    let next2Children = Children.get [] next2
                    let next1Children = Children.get [] next1
                    next2Children @ next1Children @ current
                | [ expr ] -> expr :: current
                | _ -> current
            | Let({Name=name; Range=range; Type=DeclaredType({FullName = fullName}, [])}, _, _)
                when (name.StartsWith("returnVal") && fullName.StartsWith("Oxpecker.Solid"))
                || (name.StartsWith("element") && fullName.StartsWith("Oxpecker.Solid"))
                |> not ->
                match range with
                | Some range -> failwith $"let binding inside HTML CE can't be converted"
                | None -> failwith "let binding inside HTML CE can't be converted"
            | _ -> current
        let wrap childrenExpression =
            Value(
                kind = NewTuple(
                    values = [
                        Value(kind=StringConstant "children", range=None)
                        TypeCast(childrenExpression, Type.Any)
                    ],
                    isStruct = false
                    ),
                range = None
            )
    module Helpers =
        let convertListToExpr (exprs: Fable.Expr list) =
            (Baked.emptyList, exprs)
            ||> List.fold(fun acc prop ->
                Value(kind = NewList(headAndTail = Some(prop, acc), typ = Baked.listItemType), range = None))

    let transform expr =
        match expr with
        | Let.NoChildrenWithProps builder
        | Let.Element & Let(_, Let.NoChildrenWithProps builder, _) -> builder |> TagBuilder.transform
        | Tag.NoChildren builder ->
            builder
            |> TagBuilder.info (TagBuilderInfo.create [])
            |> TagBuilder.transform
        | Tag.NoChildrenWithHandler builder
        | Let.NoChildrenNoProps builder
        | Tag.WithChildren builder
        | Let.WithChildren builder -> builder |> TagBuilder.transform
        | Call.TagImport (tagSource, range) ->
            TagBuilder.create (LibraryImport tagSource, TagBuilderInfo.create [], range)
            |>  TagBuilder.transform
        | Let(_, Call.TagImport(tagSource, range), Tag.WithChildren builder) ->
            builder
            |> TagBuilder.tag (LibraryImport tagSource)
            |> TagBuilder.range range
            |> TagBuilder.transform
        | Let(_, Call.TagImport(tagSource, range), Sequential exprs) ->
            TagBuilder.create (LibraryImport tagSource, TagBuilderInfo.create exprs, range)
            |> TagBuilder.transform
        | Let(_, Let(_, Call.TagImport(tagSource, range), Sequential exprs), Tag.WithChildren builder) ->
            builder
            |> TagBuilder.range range
            |> TagBuilder.tag (LibraryImport tagSource)
            |> TagBuilder.info.adds exprs
            |> TagBuilder.transform
        | Sequential expressions ->
            Sequential (
                expressions
                |> List.mapi(fun i expr -> if i = expressions.Length - 1 then transform expr else expr)
            )
        | Call(callee, callInfo, typ, range) ->
            {
                callInfo with Args = callInfo.Args |> List.map transform
            }
            |> fun callInfo -> Call(callee, callInfo, typ, range)
        | Lambda.TextNoSiblings body -> body
        | IfThenElse(guardExpr, thenExpr, elseExpr, range) ->
            IfThenElse(guardExpr, transform thenExpr, transform elseExpr, range)
        | DecisionTree(decisionTree, targets) ->
            DecisionTree(decisionTree, targets |> List.map(fun (target, expr) -> target, transform expr))
        | TypeCast(expr, DeclaredType _) -> transform expr
        | _ -> expr

    let transformException (pluginHelper: PluginHelper) (range: SourceLocation option) =
        let childrenExpression =
            Value(
                NewList(
                    Some(
                        Value(StringConstant $"Fable compilation error in {pluginHelper.CurrentFile}", None),
                        Value(NewList(None, Type.Tuple([ Type.String; Type.Any ], false)), None)
                    ),
                    Type.Tuple([ Type.String; Type.Any ], false)
                ),
                None
            )
        let text = Children.wrap childrenExpression
        let finalList = text :: []
        let propsExpr = Helpers.convertListToExpr finalList
        Call(
            callee = Baked.importJsxCreate,
            info = {
                ThisArg = None
                Args = [ Value(StringConstant "h1", None); propsExpr ]
                SignatureArgTypes = []
                GenericArgs = []
                MemberRef = None
                Tags = [ "jsx" ]
            },
            typ = Baked.jsxElementType,
            range = range
        )
open FSharp.Json
/// <summary>
/// Registers a function as a SolidComponent for transformation by
/// the <c>Oxpecker.Solid.FablePlugin</c>
/// </summary>
type SolidComponentAttribute() =
    inherit MemberDeclarationPluginAttribute()

    override _.FableMinimumVersion = "4.0"

    override this.Transform(pluginHelper: PluginHelper, file: File, memberDecl: MemberDecl) =
        Console.ForegroundColor <- ConsoleColor.Cyan
        Console.WriteLine memberDecl.Body
        let newBody =
            match memberDecl.Body with
            | Extended(Throw _, range) -> AST.transformException pluginHelper range
            | _ -> AST.transform memberDecl.Body
        { memberDecl with Body = newBody }

    override _.TransformCall(_: PluginHelper, _: MemberFunctionOrValue, expr: Expr) : Expr = expr
