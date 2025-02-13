namespace Oxpecker.Solid

open System
open Fable
open Fable.AST
open Fable.AST.Fable

[<assembly: ScanForPlugins>]
do () // Prompts fable to utilise this plugin

module internal rec AST =
    /// <summary>
    /// AST Representation for a JSX Attribute/property. Tuple of name and value
    /// </summary>
    type PropInfo = string * Expr
    /// <summary>
    /// List of AST property name value pairs
    /// </summary>
    type Props = PropInfo list

    type TagSource =
        | AutoImport of tagName: string
        | LibraryImport of imp: Expr
    /// <summary>
    /// DU which distinguishes between a user call instantiating the tag with children, without children (props only),
    /// or with both children AND properties.
    /// </summary>
    type TagInfo =
        | WithChildren of tagName: TagSource * propsAndChildren: CallInfo * range: SourceLocation option
        | NoChildren of tagName: TagSource * props: Expr list * range: SourceLocation option
        | Combined of tagName: TagSource * props: Expr list * propsAndChildren: CallInfo * range: SourceLocation option
    [<AutoOpen>]
    module Native =
        let (|StartsWith|_|) (value: string) : string -> unit option = function
            | s when s.StartsWith(value) -> Some()
            | _ -> None
        let (|EndsWith|_|) (value: string) : string -> unit option = function
            | s when s.EndsWith(value) -> Some()
            | _ -> None
    [<RequireQualifiedAccess>]
    module EntityRef =
        let (|StartsWith|_|) (value : string) = function
            | e when e.FullName.StartsWith value -> Some()
            | _ -> None
    [<RequireQualifiedAccess>]
    module Ident =
        let (|StartsWith|_|) (value : string) : Ident -> unit option = function
            | i when i.Name.StartsWith value -> Some()
            | _ -> None
    [<RequireQualifiedAccess>]
    module ImportInfo =
        let (|StartsWith|_|) (value : string) = function
            | i when i.Selector.StartsWith value -> Some()
            | _ -> None
    [<RequireQualifiedAccess>]
    module Import =
        let (|Info|_|) = function | Import(value,_,_) -> Some value | _ -> None
        let (|Type|_|) = function | Import(_,value,_) -> Some value | _ -> None
        let (|Range|_|) = function | Import(_,_,value) -> Some value | _ -> None
    [<RequireQualifiedAccess>]
    module Call =
        /// <summary>
        /// Matches expressions for tags that are imported from a namespace starting with <c>Oxpecker.Solid</c>
        /// </summary>
        /// <returns><c>Expr * SourceLocation</c></returns>
        let (|ImportTag|_|) (expr: Expr) =
            match expr with
            | Call(Import({ Kind = UserImport false }, Any, _) as imp, { Tags = [ "new" ] }, DeclaredType(EntityRef.StartsWith "Oxpecker.Solid", []), range) ->
                Some(imp, range)
            | _ -> None
        /// <summary>
        /// Pattern matches expressions for Tags calls.
        /// </summary>
        /// <param name="condition"><c>ImportInfo</c></param>
        /// <remarks>Apostrophised tagnames are cleaned of the apostrophe during transpilation</remarks>
        /// <returns><c>TagSource * CallInfo * SourceLocation option</c></returns>
        let (|Tag|_|) condition =
            function
            | Call(Import(importInfo, LambdaType(_, DeclaredType(typ, _)), _), callInfo, _, range) when condition importInfo ->
                let tagImport =
                    match callInfo.Args with
                    | Call.ImportTag(imp, _) :: _
                    | Let(_, Call.ImportTag(imp, _), _) :: _ -> LibraryImport imp
                    | _ ->
                        let tagName = typ.FullName.Split('.') |> Seq.last
                        let finalTagName =
                            if tagName = "Fragment" then
                                ""
                            elif tagName.EndsWith("'") then
                                tagName.Substring(0, tagName.Length - 1)
                            elif tagName.EndsWith("`1") then
                                tagName.Substring(0, tagName.Length - 2)
                            else
                                tagName
                        AutoImport finalTagName
                Some(tagImport, callInfo, range)
            | _ -> None

    [<RequireQualifiedAccess>]
    module Tag =
        /// <summary>
        /// Pattern matches expressions to Tags calls without children
        /// </summary>
        /// <returns><c>TagInfo.NoChildren</c></returns>
        let (|NoChildren|_|) (expr: Expr) =
            let condition = _.Selector.EndsWith("_$ctor")
            match expr with
            | Call.Tag condition (tagName, _, range) -> Some(tagName, range)
            | _ -> None
        /// <summary>
        /// Pattern matches expressions to Tag calls with children
        /// </summary>
        /// <returns><c>TagInfo.WithChildren</c></returns>
        let (|WithChildren|_|) (expr: Expr) =
            let condition =
                fun importInfo ->
                    importInfo.Selector.StartsWith("HtmlContainerExtensions_Run")
                    || importInfo.Selector.StartsWith("BindingsModule_Extensions_Run")
            match expr with
            | Call.Tag condition tagCallInfo -> Some tagCallInfo
            | _ -> None
    [<RequireQualifiedAccess>]
    module Let =
        let (|Ident|_|) = function | Let(value,_,_) -> Some value | _ -> None
        let (|Value|_|) = function | Let(_,value,_) -> Some value | _ -> None
        let (|Body|_|) = function | Let(_,_,value) -> Some value | _ -> None
        /// <summary>
        /// Pattern matches <c>let</c> bindings that start with <c>element</c>
        /// </summary>
        /// <returns><c>unit</c></returns>
        let (|Element|_|) =
            function
            | Let.Ident (Ident.StartsWith "element") -> Some()
            | _ -> None
        /// <summary>
        /// Pattern matches <c>let</c> bindings for Tags with children
        /// </summary>
        /// <returns><c>TagInfo.WithChildren</c></returns>
        let (|TagWithChildren|_|) =
            function
            | Let.Value (Tag.WithChildren(tagName, callInfo, range)) -> TagInfo.WithChildren(tagName, callInfo, range) |> Some
            | _ -> None
        /// <summary>
        /// Pattern matches <c>let</c> bindings for Tags without children (but with props)
        /// </summary>
        /// <returns><c>TagInfo.NoChildren</c></returns>
        let (|TagNoChildrenWithProps|_|) =
            function
            | Let(_, Tag.NoChildren(tagName, range), Sequential exprs) -> TagInfo.NoChildren(tagName, exprs, range) |> Some
            | _ -> None
    /// <summary>
    /// Pattern matches expressions (<c>let</c> or otherwise) for tags without children directly to Tag calls
    /// </summary>
    /// <returns><c>TagInfo.NoChildren</c></returns>
    let (|CallTagNoChildrenWithHandler|_|) (expr: Expr) =
        match expr with
        | Call(Import.Info (ImportInfo.StartsWith "HtmlElementExtensions_"), { Args = args :: _ }, _, range) -> Some (args,range)
        | _ -> None
        |> Option.bind (function
            | Tag.NoChildren(tagName, _), range ->
                TagInfo.NoChildren(tagName, [ expr ], range) |> Some
            | Let.TagNoChildrenWithProps(NoChildren(tagName, props, _)), range ->
                TagInfo.NoChildren(tagName, expr :: props, range) |> Some
            | CallTagNoChildrenWithHandler(NoChildren(tagName, props, _)), range ->
                TagInfo.NoChildren(tagName, expr :: props, range) |> Some
            | _ -> None
            )
    /// <summary>
    /// Pattern matches <c>let</c> bindings for tags without children or props
    /// </summary>
    /// <returns><c>TagInfo.NoChildren</c></returns>
    let (|LetTagNoChildrenNoProps|_|) =
        function
        | Let.Value (Tag.NoChildren(tagName, range)) -> TagInfo.NoChildren(tagName, [], range) |> Some
        | _ -> None
    /// <summary>
    /// Pattern matches expressions that are text in isolation (no siblings)
    /// </summary>
    /// <returns><c>Expr</c> of text</returns>
    let (|TextNoSiblings|_|) =
        function
        | Lambda(Ident.StartsWith "cont", TypeCast(textBody, Unit), None) -> Some textBody
        | _ -> None
    /// <summary>
    /// Plugin type declaration for JSX Element
    /// </summary>
    let jsxElementType =
        Type.DeclaredType(
            ref = {
                FullName = "Fable.Core.JSX.Element"
                Path = EntityPath.CoreAssemblyName "Fable.Core"
            },
            genericArgs = []
        )
    /// <summary>
    /// Plugin import declaration for JSX <c>create</c>
    /// </summary>
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


    let private (|EventHandler|_|) callInfo =
        match callInfo with
        | {
              Args = [ _; Value(StringConstant eventName, _); handler ]
          } -> Some(eventName, handler)
        | _ -> None

    let rec collectAttributes (exprs: Expr list) =
        match exprs with
        | [] -> []
        | Sequential expressions :: rest -> collectAttributes rest @ collectAttributes expressions
        | Call(Import.Info importInfo, callInfo, _, _) :: rest ->
            let restResults = collectAttributes rest
            match importInfo.Kind with
            | ImportKind.MemberImport(MemberRef(EntityRef.StartsWith "Oxpecker.Solid", memberRefInfo)) ->
                match memberRefInfo.CompiledName, callInfo with
                | "on", EventHandler(eventName, handler) -> ("on:" + eventName, handler) :: restResults
                | "bool", EventHandler(eventName, handler) -> ("bool:" + eventName, handler) :: restResults
                | "data", EventHandler(eventName, handler) -> ("data-" + eventName, handler) :: restResults
                | "attr", EventHandler(eventName, handler) -> (eventName, handler) :: restResults
                | "ref", { Args = [ _; identExpr ] } -> ("ref", identExpr) :: restResults
                | "style'", { Args = [ _; identExpr ] } -> ("style", identExpr) :: restResults
                | "classList", { Args = [ _; identExpr ] } -> ("classList", identExpr) :: restResults
                | _ ->
                    let setterIndex = memberRefInfo.CompiledName.IndexOf("set_")
                    if setterIndex >= 0 then
                        let propName =
                            match memberRefInfo.CompiledName.Substring(setterIndex + "set_".Length) with
                            | name when name.EndsWith("'") -> name.Substring(0, name.Length - 1) // like class' or type'
                            | name when name.StartsWith("aria") -> $"aria-{name.Substring(4).ToLower()}"
                            | name -> name
                        let propValue = callInfo.Args.Head
                        match propValue with
                        | TypeCast(expr,
                                   DeclaredType({
                                                    FullName = "Oxpecker.Solid.Builder.HtmlElement"
                                                },
                                                _)) -> (propName, transform expr) :: restResults
                        | Delegate(args, expr, name, tags) ->
                            (propName, Delegate(args, transform expr, name, tags)) :: restResults
                        | _ -> (propName, propValue) :: restResults
                    else
                        restResults
            | _ -> restResults
        | Set(IdentExpr(Ident.StartsWith "returnVal"), SetKind.FieldSet name, _, handler, _) :: rest ->
            let propName =
                match name with
                | name when name.EndsWith("'") -> name.Substring(0, name.Length - 1) // like class' or type'
                | name -> name
            (propName, handler) :: collectAttributes rest
        | _ :: rest -> collectAttributes rest

    let getAttributes currentList (expr: Expr) : Props =
        match expr with
        | Let(Ident.StartsWith "returnVal", _, Sequential exprs) ->
            collectAttributes exprs @ currentList
        | CallTagNoChildrenWithHandler(NoChildren(_, props, _)) -> collectAttributes props @ currentList
        | _ -> currentList

    let getChildren currentList (expr: Expr) : Expr list =
        match expr with
        | Let.TagWithChildren tagInfo ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | Let.Element & Let.Value (Let.TagNoChildrenWithProps tagInfo) ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | Let.Element & LetTagNoChildrenNoProps tagInfo ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | Let.Element & Let.Value (CallTagNoChildrenWithHandler tagInfo) ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | Let.Element & Let.Value next ->
            let newExpr = transform next
            newExpr :: currentList
        // Lambda with two arguments returning element
        | Lambda(Ident.StartsWith "cont", TypeCast(Lambda(item, Lambda(index, next, _), _), _), _) ->
            Delegate([ item; index ], transform next, None, []) :: currentList
        | TextNoSiblings body -> body :: currentList
        // text with solid signals inside
        | Let(Ident.StartsWith "text", body, TextNoSiblings _) -> body :: currentList
        // text then tag
        | Let(Ident.StartsWith "second", next, Lambda(Ident.StartsWith "builder", Sequential(TypeCast(textBody, Unit) :: _), _)) ->
            getChildren (textBody :: currentList) next
        // parameter then another parameter
        | CurriedApply(Lambda(Ident.StartsWith "cont", TypeCast(lastParameter, Unit), Some (StartsWith "second")), _, _, _) ->
            lastParameter :: currentList
        | CurriedApply(Lambda(Ident.StartsWith "builder", Sequential [ TypeCast(middleParameter, Unit); next ], _), _, _, _)
        | Lambda(Ident.StartsWith "builder", Sequential [ TypeCast(middleParameter, Unit); next ], _) ->
            getChildren (middleParameter :: currentList) next
        // tag then text
        | Let(Ident.StartsWith "first", next1, Lambda(Ident.StartsWith "builder", Sequential [ CurriedApply _; next2 ], _)) ->
            let next2Children = getChildren [] next2
            let next1Children = getChildren [] next1
            next2Children @ next1Children @ currentList
        | Let(Ident.StartsWith "first", Let.Value expr, Let(Ident.StartsWith "second", next, _))
        | Let(Ident.StartsWith "first", expr, Let(Ident.StartsWith "second", next, _)) ->
            match expr with
            | Let.TagNoChildrenWithProps tagInfo ->
                let newExpr = transformTagInfo tagInfo
                getChildren (newExpr :: currentList) next
            | Tag.NoChildren(tagName, range) ->
                let newExpr = transformTagInfo(TagInfo.NoChildren(tagName, [], range))
                getChildren (newExpr :: currentList) next
            | Tag.WithChildren callInfo ->
                let newExpr = transformTagInfo(TagInfo.WithChildren callInfo)
                getChildren (newExpr :: currentList) next
            | expr ->
                let newExpr = transform expr
                getChildren (newExpr :: currentList) next
        | IfThenElse(guardExpr, thenExpr, elseExpr, range) ->
            IfThenElse(guardExpr, transform thenExpr, transform elseExpr, range)
            :: currentList
        | DecisionTree(decisionTree, targets) ->
            DecisionTree(decisionTree, targets |> List.map(fun (target, expr) -> target, transform expr))
            :: currentList
        // router cases
        | Call(Get(IdentExpr _, FieldGet _, Any, _), { Args = args }, _, _) ->
            match args with
            | [ Call(Import({ Selector = "uncurry2" }, Any, None), { Args = [ Lambda(_, body, _) ] }, _, _) ] ->
                getChildren currentList body
            | [ Call.ImportTag(imp, _) ] ->
                let newExpr = transformTagInfo(TagInfo.NoChildren(LibraryImport imp, [], None))
                newExpr :: currentList
            | [ Let(_, Call.ImportTag(imp, _), Sequential exprs) ] ->
                let newExpr = transformTagInfo(TagInfo.NoChildren(LibraryImport imp, exprs, None))
                newExpr :: currentList
            | [ Let(_, Let(_, Call.ImportTag(imp, _), Sequential exprs), Tag.WithChildren(_, callInfo, _)) ] ->
                let newExpr =
                    transformTagInfo(TagInfo.Combined(LibraryImport imp, exprs, callInfo, None))
                newExpr :: currentList
            | [ next1; next2 ] ->
                let next2Children = getChildren [] next2
                let next1Children = getChildren [] next1
                next2Children @ next1Children @ currentList
            | [ expr ] -> expr :: currentList
            | _ -> currentList
        | Let.Ident { Name = name
                      Range = range
                      Type = DeclaredType(EntityRef.StartsWith "Oxpecker.Solid", []) }
            when not (name.StartsWith("returnVal") || name.StartsWith("element")) ->
                match range with
                | Some range -> failwith $"`let` binding inside HTML CE can't be converted to JSX:line {range.start.line}"
                | None -> failwith $"`let` binding inside HTML CE can't be converted to JSX"
        | _ -> currentList

    let listItemType =
        Type.Tuple(genericArgs = [ Type.String; Type.Any ], isStruct = false)
    let emptyList =
        Value(kind = NewList(headAndTail = None, typ = listItemType), range = None)

    let convertExprListToExpr (exprs: Expr list) =
        (emptyList, exprs)
        ||> List.fold(fun acc prop ->
            Value(kind = NewList(headAndTail = Some(prop, acc), typ = listItemType), range = None))

    let wrapChildrenExpression childrenExpression =
        Value(
            kind =
                NewTuple(
                    values = [
                        // property name
                        Value(kind = StringConstant "children", range = None)
                        // property value
                        TypeCast(childrenExpression, Type.Any)
                    ],
                    isStruct = false
                ),
            range = None
        )

    let transformTagInfo (tagInfo: TagInfo) : Expr =
        let tagName, props, children, range =
            match tagInfo with
            | WithChildren(tagName, callInfo, range) ->
                let props = callInfo.Args |> List.fold getAttributes []
                let childrenList = callInfo.Args |> List.fold getChildren []
                tagName, props, childrenList, range
            | NoChildren(tagName, propList, range) ->
                let props = collectAttributes propList
                let childrenList = []
                tagName, props, childrenList, range
            | Combined(tagName, propList, callInfo, range) ->
                let props = collectAttributes propList
                let childrenList = callInfo.Args |> List.fold getChildren []
                tagName, props, childrenList, range

        let propsXs =
            props
            |> List.map(fun (name, expr) ->
                Value(
                    kind =
                        NewTuple(
                            values = [
                                // property name
                                Value(kind = StringConstant name, range = None)
                                // property value
                                TypeCast(expr, Type.Any)
                            ],
                            isStruct = false
                        ),
                    range = None
                ))
        let childrenExpression = convertExprListToExpr children

        let finalList = (wrapChildrenExpression childrenExpression) :: propsXs
        let propsExpr = convertExprListToExpr finalList
        let tag =
            match tagName with
            | AutoImport tagName -> Value(StringConstant tagName, None)
            | LibraryImport imp -> imp

        Call(
            callee = importJsxCreate,
            info = {
                ThisArg = None
                Args = [ tag; propsExpr ]
                SignatureArgTypes = []
                GenericArgs = []
                MemberRef = None
                Tags = [ "jsx" ]
            },
            typ = jsxElementType,
            range = range
        )

    let transform (expr: Expr) =
        match expr with
        | Let.TagNoChildrenWithProps tagCall
        | Let.Element & Let(_, Let.TagNoChildrenWithProps tagCall, _) -> transformTagInfo tagCall
        | Tag.NoChildren(tagName, range) -> transformTagInfo(TagInfo.NoChildren(tagName, [], range))
        | CallTagNoChildrenWithHandler tagCall -> transformTagInfo tagCall
        | LetTagNoChildrenNoProps tagCall -> transformTagInfo tagCall
        | Tag.WithChildren callInfo -> transformTagInfo(TagInfo.WithChildren callInfo)
        | Let.TagWithChildren tagCall -> transformTagInfo tagCall
        | Call.ImportTag(imp, range) -> transformTagInfo(TagInfo.NoChildren(LibraryImport imp, [], range))
        | Let(_, Call.ImportTag(imp, range), Tag.WithChildren(_, callInfo, _)) ->
            transformTagInfo(TagInfo.WithChildren(LibraryImport imp, callInfo, range))
        | Let(_, Call.ImportTag(imp, range), Sequential exprs) ->
            transformTagInfo(TagInfo.NoChildren(LibraryImport imp, exprs, range))
        | Let(_, Let(_, Call.ImportTag(imp, range), Sequential exprs), Tag.WithChildren(_, callInfo, _)) ->
            transformTagInfo(TagInfo.Combined(LibraryImport imp, exprs, callInfo, range))
        | Let(name, value, expr) -> Let(name, value, (transform expr))
        | Sequential expressions ->
            // transform only the last expression
            Sequential(
                expressions
                |> List.mapi(fun i expr -> if i = expressions.Length - 1 then transform expr else expr)
            )
        // transform children passed to component function call
        | Call(callee, callInfo, typ, range) ->
            let newCallInfo = {
                callInfo with
                    Args = callInfo.Args |> List.map transform
            }
            Call(callee, newCallInfo, typ, range)
        | TextNoSiblings body -> body
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
        let text = wrapChildrenExpression childrenExpression
        let finalList = text :: []
        let propsExpr = convertExprListToExpr finalList
        Call(
            callee = importJsxCreate,
            info = {
                ThisArg = None
                Args = [ Value(StringConstant "h1", None); propsExpr ]
                SignatureArgTypes = []
                GenericArgs = []
                MemberRef = None
                Tags = [ "jsx" ]
            },
            typ = jsxElementType,
            range = range
        )

/// <summary>
/// Registers a function as a SolidComponent for transformation by
/// the <c>Oxpecker.Solid.FablePlugin</c>
/// </summary>
type SolidComponentAttribute() =
    inherit MemberDeclarationPluginAttribute()
    override _.FableMinimumVersion = "4.0"

    override this.Transform(pluginHelper: PluginHelper, file: File, memberDecl: MemberDecl) =
        // Console.WriteLine("!Start! MemberDecl")
        // Console.WriteLine(memberDecl.Body)
        // Console.WriteLine("!End! MemberDecl")
        let newBody =
            match memberDecl.Body with
            | Extended(Throw _, range) -> AST.transformException pluginHelper range
            | _ -> AST.transform memberDecl.Body
        { memberDecl with Body = newBody }

    override _.TransformCall(_: PluginHelper, _: MemberFunctionOrValue, expr: Expr) : Expr = expr
