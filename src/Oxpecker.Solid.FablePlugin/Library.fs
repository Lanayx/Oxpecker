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

    /// <summary>
    /// Pattern matches expressions for Tags calls.
    /// </summary>
    /// <param name="condition"><c>ImportInfo</c></param>
    /// <remarks>Apostrophised tagnames are cleaned of the apostrophe during transpilation</remarks>
    /// <returns><c>TagSource * CallInfo * SourceLocation option</c></returns>
    let (|CallTag|_|) condition =
        function
        | Call(Import(importInfo, LambdaType(_, DeclaredType(typ, _)), _), callInfo, _, range) when condition importInfo ->
            let tagImport =
                match callInfo.Args with
                | LibraryTagImport(imp, _) :: _
                | Let(_, LibraryTagImport(imp, _), _) :: _ -> LibraryImport imp
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
    /// <summary>
    /// Pattern matches expressions to Tags calls without children
    /// </summary>
    /// <returns><c>TagInfo.NoChildren</c></returns>
    let (|TagNoChildren|_|) (expr: Expr) =
        let condition = _.Selector.EndsWith("_$ctor")
        match expr with
        | CallTag condition (tagName, _, range) -> Some(tagName, range)
        | _ -> None
    /// <summary>
    /// Pattern matches expressions to Tag calls with children
    /// </summary>
    /// <returns><c>TagInfo.WithChildren</c></returns>
    let (|TagWithChildren|_|) (expr: Expr) =
        let condition =
            fun importInfo ->
                importInfo.Selector.StartsWith("HtmlContainerExtensions_Run")
                || importInfo.Selector.StartsWith("BindingsModule_Extensions_Run")
        match expr with
        | CallTag condition tagCallInfo -> Some tagCallInfo
        | _ -> None
    /// <summary>
    /// Pattern matches <c>let</c> bindings that start with <c>element</c>
    /// </summary>
    /// <returns><c>unit</c></returns>
    let (|LetElement|_|) =
        function
        | Let({ Name = name }, _, _) when name.StartsWith("element") -> Some()
        | _ -> None
    /// <summary>
    /// Pattern matches <c>let</c> bindings for Tags with children
    /// </summary>
    /// <returns><c>TagInfo.WithChildren</c></returns>
    let (|LetTagWithChildren|_|) =
        function
        | Let(_, TagWithChildren(tagName, callInfo, range), _) -> TagInfo.WithChildren(tagName, callInfo, range) |> Some
        | _ -> None
    /// <summary>
    /// Pattern matches <c>let</c> bindings for Tags without children (but with props)
    /// </summary>
    /// <returns><c>TagInfo.NoChildren</c></returns>
    let (|LetTagNoChildrenWithProps|_|) =
        function
        | Let(_, TagNoChildren(tagName, range), Sequential exprs) -> TagInfo.NoChildren(tagName, exprs, range) |> Some
        | _ -> None
    /// <summary>
    /// Pattern matches expressions (<c>let</c> or otherwise) for tags without children directly to Tag calls
    /// </summary>
    /// <returns><c>TagInfo.NoChildren</c></returns>
    let (|CallTagNoChildrenWithHandler|_|) (expr: Expr) =
        match expr with
        | Call(Import(importInfo, _, _),
               {
                   Args = TagNoChildren(tagName, _) :: _
               },
               _,
               range) when importInfo.Selector.StartsWith("HtmlElementExtensions_") -> // on, attr, data, ref
            TagInfo.NoChildren(tagName, [ expr ], range) |> Some
        | Call(Import(importInfo, _, _),
               {
                   Args = LetTagNoChildrenWithProps(NoChildren(tagName, props, _)) :: _
               },
               _,
               range) when importInfo.Selector.StartsWith("HtmlElementExtensions_") -> // on, attr, data, ref
            TagInfo.NoChildren(tagName, expr :: props, range) |> Some
        | Call(Import(importInfo, _, _),
               {
                   Args = CallTagNoChildrenWithHandler(NoChildren(tagName, props, _)) :: _
               },
               _,
               range) when importInfo.Selector.StartsWith("HtmlElementExtensions_") -> // on, attr, data, ref
            TagInfo.NoChildren(tagName, expr :: props, range) |> Some
        | _ -> None
    /// <summary>
    /// Pattern matches <c>let</c> bindings for tags without children or props
    /// </summary>
    /// <returns><c>TagInfo.NoChildren</c></returns>
    let (|LetTagNoChildrenNoProps|_|) =
        function
        | Let(_, TagNoChildren(tagName, range), _) -> TagInfo.NoChildren(tagName, [], range) |> Some
        | _ -> None
    /// <summary>
    /// Pattern matches expressions that are text in isolation (no siblings)
    /// </summary>
    /// <returns><c>Expr</c> of text</returns>
    let (|TextNoSiblings|_|) =
        function
        | Lambda({ Name = cont }, TypeCast(textBody, Unit), None) when cont.StartsWith("cont") -> Some textBody
        | _ -> None
    /// <summary>
    /// Matches expressions for tags that are imported from a namespace starting with <c>Oxpecker.Solid</c>
    /// </summary>
    /// <returns><c>Expr * SourceLocation</c></returns>
    let (|LibraryTagImport|_|) (expr: Expr) =
        match expr with
        | Call(Import({ Kind = UserImport false }, Any, _) as imp, { Tags = [ "new" ] }, DeclaredType(typ, []), range) when
            typ.FullName.StartsWith("Oxpecker.Solid")
            ->
            Some(imp, range)
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
        | Call(Import(importInfo, _, _), callInfo, _, _) :: rest ->
            let restResults = collectAttributes rest
            match importInfo.Kind with
            | ImportKind.MemberImport(MemberRef(entity, memberRefInfo)) when
                entity.FullName.StartsWith("Oxpecker.Solid")
                ->
                match memberRefInfo.CompiledName, callInfo with
                | "on", EventHandler(eventName, handler) -> ("on:" + eventName, handler) :: restResults
                | "bool", EventHandler(eventName, handler) -> ("bool:" + eventName, handler) :: restResults
                | "data", EventHandler(eventName, handler) -> ("data-" + eventName, handler) :: restResults
                | "attr", EventHandler(eventName, handler) -> (eventName, handler) :: restResults
                | "spread", { Args = [ _; identExpr ] } -> ("__SPREAD_PROPERTY__", identExpr) :: restResults
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
        | Set(IdentExpr({ Name = returnVal }), SetKind.FieldSet name, _, handler, _) :: rest when
            returnVal.StartsWith("returnVal")
            ->
            let propName =
                match name with
                | name when name.EndsWith("'") -> name.Substring(0, name.Length - 1) // like class' or type'
                | name -> name
            (propName, handler) :: collectAttributes rest
        | _ :: rest -> collectAttributes rest

    let getAttributes currentList (expr: Expr) : Props =
        match expr with
        | Let({ Name = returnVal }, _, Sequential exprs) when returnVal.StartsWith("returnVal") ->
            collectAttributes exprs @ currentList
        | CallTagNoChildrenWithHandler(NoChildren(_, props, _)) -> collectAttributes props @ currentList
        | _ -> currentList

    let getChildren currentList (expr: Expr) : Expr list =
        match expr with
        | LetTagWithChildren tagInfo ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | LetElement & Let(_, LetTagNoChildrenWithProps tagInfo, _) ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | LetElement & LetTagNoChildrenNoProps tagInfo ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | LetElement & Let(_, CallTagNoChildrenWithHandler tagInfo, _) ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | LetElement & Let(_, next, _) ->
            let newExpr = transform next
            newExpr :: currentList
        // Lambda with two arguments returning element
        | Lambda({ Name = cont }, TypeCast(Lambda(item, Lambda(index, next, _), _), _), _) when cont.StartsWith("cont") ->
            Delegate([ item; index ], transform next, None, []) :: currentList
        | TextNoSiblings body -> body :: currentList
        // text with solid signals inside
        | Let({ Name = text }, body, TextNoSiblings _) when text.StartsWith("text") -> body :: currentList
        // text then tag
        | Let({ Name = second }, next, Lambda({ Name = builder }, Sequential(TypeCast(textBody, Unit) :: _), _)) when
            second.StartsWith("second") && builder.StartsWith("builder")
            ->
            getChildren (textBody :: currentList) next
        // parameter then another parameter
        | CurriedApply(Lambda({ Name = cont }, TypeCast(lastParameter, Unit), Some second), _, _, _) when
            cont.StartsWith("cont") && second.StartsWith("second")
            ->
            lastParameter :: currentList
        | CurriedApply(Lambda({ Name = builder }, Sequential [ TypeCast(middleParameter, Unit); next ], _), _, _, _)
        | Lambda({ Name = builder }, Sequential [ TypeCast(middleParameter, Unit); next ], _) when
            builder.StartsWith("builder")
            ->
            getChildren (middleParameter :: currentList) next
        // tag then text
        | Let({ Name = first }, next1, Lambda({ Name = builder }, Sequential [ CurriedApply _; next2 ], _)) when
            first.StartsWith("first") && builder.StartsWith("builder")
            ->
            let next2Children = getChildren [] next2
            let next1Children = getChildren [] next1
            next2Children @ next1Children @ currentList
        | Let({ Name = first }, Let(_, expr, _), Let({ Name = second }, next, _))
        | Let({ Name = first }, expr, Let({ Name = second }, next, _)) when
            first.StartsWith("first") && second.StartsWith("second")
            ->
            match expr with
            | LetTagNoChildrenWithProps tagInfo ->
                let newExpr = transformTagInfo tagInfo
                getChildren (newExpr :: currentList) next
            | TagNoChildren(tagName, range) ->
                let newExpr = transformTagInfo(TagInfo.NoChildren(tagName, [], range))
                getChildren (newExpr :: currentList) next
            | TagWithChildren callInfo ->
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
            | [ LibraryTagImport(imp, _) ] ->
                let newExpr = transformTagInfo(TagInfo.NoChildren(LibraryImport imp, [], None))
                newExpr :: currentList
            | [ Let(_, LibraryTagImport(imp, _), Sequential exprs) ] ->
                let newExpr = transformTagInfo(TagInfo.NoChildren(LibraryImport imp, exprs, None))
                newExpr :: currentList
            | [ Let(_, Let(_, LibraryTagImport(imp, _), Sequential exprs), TagWithChildren(_, callInfo, _)) ] ->
                let newExpr =
                    transformTagInfo(TagInfo.Combined(LibraryImport imp, exprs, callInfo, None))
                newExpr :: currentList
            | [ next1; next2 ] ->
                let next2Children = getChildren [] next2
                let next1Children = getChildren [] next1
                next2Children @ next1Children @ currentList
            | [ expr ] -> expr :: currentList
            | _ -> currentList
        | Let({
                  Name = name
                  Range = range
                  Type = DeclaredType({ FullName = fullName }, [])
              },
              _,
              _) when
            ((name.StartsWith("returnVal") && fullName.StartsWith("Oxpecker.Solid"))
             || (name.StartsWith("element") && fullName.StartsWith("Oxpecker.Solid")))
            |> not
            ->
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
        | LetTagNoChildrenWithProps tagCall
        | LetElement & Let(_, LetTagNoChildrenWithProps tagCall, _) -> transformTagInfo tagCall
        | TagNoChildren(tagName, range) -> transformTagInfo(TagInfo.NoChildren(tagName, [], range))
        | CallTagNoChildrenWithHandler tagCall -> transformTagInfo tagCall
        | LetTagNoChildrenNoProps tagCall -> transformTagInfo tagCall
        | TagWithChildren callInfo -> transformTagInfo(TagInfo.WithChildren callInfo)
        | LetTagWithChildren tagCall -> transformTagInfo tagCall
        | LibraryTagImport(imp, range) -> transformTagInfo(TagInfo.NoChildren(LibraryImport imp, [], range))
        | Let(_, LibraryTagImport(imp, range), TagWithChildren(_, callInfo, _)) ->
            transformTagInfo(TagInfo.WithChildren(LibraryImport imp, callInfo, range))
        | Let(_, LibraryTagImport(imp, range), Sequential exprs) ->
            transformTagInfo(TagInfo.NoChildren(LibraryImport imp, exprs, range))
        | Let(_, Let(_, LibraryTagImport(imp, range), Sequential exprs), TagWithChildren(_, callInfo, _)) ->
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
