namespace Oxpecker.Solid

open System
open Fable
open Fable.AST
open Fable.AST.Fable

[<assembly: ScanForPlugins>]
do ()

module internal rec AST =
    type PropInfo = string * Expr
    type Props = PropInfo list

    type TagInfo =
        | WithChildren of tagName:string * propsAndChildren:CallInfo * range:SourceLocation option
        | NoChildren of tagName:string * props:Expr list * range:SourceLocation option

    let (|CallTag|_|) condition =
        function
        | Call (Import (importInfo, LambdaType (_, DeclaredType (typ, _)), _), callInfo, _, range)
                when condition importInfo ->
            let tagName = typ.FullName.Split('.') |> Seq.last
            let finalTagName =
                if tagName = "__" then
                    ""
                elif tagName.EndsWith("'") then
                    tagName.Substring(0, tagName.Length - 1)
                elif tagName.EndsWith("`1") then
                    tagName.Substring(0, tagName.Length - 2)
                else
                    tagName
            Some (finalTagName, callInfo, range)
        | _ ->
            None

    let (|TagNoChildren|_|) (expr: Expr) =
        let condition = _.Selector.EndsWith("_$ctor")
        match expr with
        | CallTag condition (tagName, _, range) ->
            Some (tagName, range)
        | _ ->
            None

    let (|TagWithChildren|_|) (expr: Expr) =
        let condition = _.Selector.StartsWith("HtmlContainerExtensions_Run")
        match expr with
        | CallTag condition tagCallInfo -> Some tagCallInfo
        | _ ->  None

    let (|LetElement|_|) =
        function
        | Let ({ Name = name }, _, _) when name.StartsWith("element") ->
            Some ()
        | _ ->
            None

    let (|LetTagWithChildren|_|) =
        function
        | Let (_, TagWithChildren (tagName, callInfo, range), _) ->
            TagInfo.WithChildren (tagName, callInfo, range) |> Some
        | _ ->
            None

    let (|LetTagNoChildrenWithProps|_|) =
        function
        | Let (_, TagNoChildren (tagName, range), Sequential exprs) ->
            TagInfo.NoChildren (tagName, exprs, range) |> Some
        | _ ->
            None

    let (|CallTagNoChildrenWithHandler|_|) (expr: Expr) =
        match expr with
        | Call (Import (importInfo, _, _), { Args = TagNoChildren (tagName, _) :: _ }, _, range)
                when importInfo.Selector.StartsWith("HtmlElementExtensions_on") ->
            TagInfo.NoChildren (tagName, [expr], range) |> Some
        | Call (Import (importInfo, _, _), { Args = LetTagNoChildrenWithProps (NoChildren (tagName, props, _)) :: _ }, _, range)
                when importInfo.Selector.StartsWith("HtmlElementExtensions_on") ->
            TagInfo.NoChildren (tagName, expr :: props, range) |> Some
        | Call (Import (importInfo, _, _), { Args = CallTagNoChildrenWithHandler (NoChildren (tagName, props, _)) :: _ }, _, range)
                when importInfo.Selector.StartsWith("HtmlElementExtensions_on") ->
            TagInfo.NoChildren (tagName, expr :: props, range) |> Some
        | _ ->
            None

    let (|LetTagNoChildrenNoProps|_|) =
        function
        | Let (_, TagNoChildren (tagName, range), _) ->
            TagInfo.NoChildren (tagName, [], range) |> Some
        | _ ->
            None

    let (|TextNoSiblings|_|) =
        function
        | Lambda ({ Name = cont }, TypeCast (textBody, Unit), None)
            when cont.StartsWith("cont") ->
            Some textBody
        | _ ->
            None

    let jsxElementType =
        Type.DeclaredType (
            ref =
                {
                    FullName = "Fable.Core.JSX.Element"
                    Path = EntityPath.CoreAssemblyName "Fable.Core"
                },
            genericArgs = []
        )

    let importJsxCreate =
        Import (
            info =
                {
                    Selector = "create"
                    Path = "@fable-org/fable-library-js/JSX.js"
                    Kind =
                        ImportKind.LibraryImport
                            {
                                IsInstanceMember = false
                                IsModuleMember = true
                            }
                },
            typ = Type.Any,
            range = None
        )

    let rec collectAttributes (exprs: Expr list) =
        match exprs with
        | [] -> []
        | Sequential expressions :: rest ->
            collectAttributes rest @ collectAttributes expressions
        | Call (Import (importInfo, _, _), { Args = [_ ; Value (StringConstant eventName, _) ; handler] }, _, _) :: rest ->
            let restResults = collectAttributes rest
            match importInfo.Kind with
            | ImportKind.MemberImport (MemberRef(entity, memberRefInfo))  when
                    entity.FullName.StartsWith("Oxpecker.Solid") ->
                if memberRefInfo.CompiledName = "on" then
                    ("on:" + eventName, handler) :: restResults
                else
                    restResults
            | _ ->
                restResults
        | Call (Import (importInfo, _, _), callInfo, _, _) :: rest ->
            let restResults = collectAttributes rest
            match importInfo.Kind with
            | ImportKind.MemberImport (MemberRef(entity, memberRefInfo)) when
                    entity.FullName.StartsWith("Oxpecker.Solid") ->
                let setterIndex = memberRefInfo.CompiledName.IndexOf("set_")
                if setterIndex >= 0 then
                    let propName =
                        match memberRefInfo.CompiledName.Substring(setterIndex + "set_".Length) with
                        | name when name.EndsWith("'") -> name.Substring(0, name.Length - 1) // like class' or type'
                        | name when name.StartsWith("aria") -> $"aria-{name.Substring(4).ToLower()}"
                        | name -> name
                    let propValue = callInfo.Args.Head
                    (propName, propValue) :: restResults
                else
                    restResults
            | _ ->
                restResults
        | Set (IdentExpr({ Name = returnVal }), SetKind.FieldSet propName, _, handler, _) :: rest
                when returnVal.StartsWith("returnVal") ->
            (propName, handler) :: collectAttributes rest
        | _ :: rest ->
            collectAttributes rest

    let getAttributes currentList (expr: Expr): Props =
        match expr with
        | Let ({ Name = returnVal }, _, Sequential exprs)
                when returnVal.StartsWith("returnVal") ->
            collectAttributes exprs @ currentList
        | CallTagNoChildrenWithHandler (NoChildren (_, props, _)) ->
            collectAttributes props @ currentList
        | _ ->
            currentList

    let getChildren currentList (expr: Expr): Expr list =
        match expr with
        | LetElement & LetTagWithChildren tagInfo ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | LetElement & Let (_, LetTagNoChildrenWithProps tagInfo, _)  ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | LetElement & LetTagNoChildrenNoProps tagInfo ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | LetElement & Let (_, CallTagNoChildrenWithHandler tagInfo, _) ->
            let newExpr = transformTagInfo tagInfo
            newExpr :: currentList
        | LetElement & Let (_, next, _) ->
            match next with
            // Pass children to the component
            | Call (callee, callInfo, typ, range) ->
                let newCallInfo = {
                    callInfo with
                        Args = callInfo.Args |> List.map transform
                }
                Call (callee, newCallInfo, typ, range) :: currentList
            | _ ->  next :: currentList
        // Lambda with two arguments returning element
        | Lambda ({ Name = cont }, TypeCast (Lambda(item, Lambda(index, next, _), _), _), _)
                when cont.StartsWith("cont") ->
            Delegate([item; index], transform next, None, []) :: currentList
        | TextNoSiblings body ->
            body :: currentList
        // text with solid signals inside
        | Let ({ Name = text }, body, TextNoSiblings _)
                when text.StartsWith("text") ->
            body :: currentList
        // text then tag
        | Let ({ Name = second }, next, Lambda ({ Name = builder }, Sequential (TypeCast (textBody, Unit)::_), _))
                when second.StartsWith("second") && builder.StartsWith("builder") ->
            getChildren (textBody ::currentList) next
        // parameter then another parameter
        | CurriedApply (Lambda ({ Name = cont }, TypeCast (lastParameter, Unit), Some second), _, _, _)
                when cont.StartsWith("cont") && second.StartsWith("second") ->
            lastParameter :: currentList
        | CurriedApply (Lambda ({ Name = builder }, Sequential [TypeCast (middleParameter, Unit); next], _), _, _, _)
        | Lambda ({ Name = builder }, Sequential [ TypeCast (middleParameter, Unit); next ], _)
                when builder.StartsWith("builder") ->
            getChildren (middleParameter :: currentList) next
        // tag then text
        | Let ({ Name = first }, next1, Lambda ({ Name = builder }, Sequential [ CurriedApply _; next2 ], _))
                when first.StartsWith("first") && builder.StartsWith("builder") ->
            let next2Children = getChildren [] next2
            let next1Children = getChildren [] next1
            next2Children @ next1Children @ currentList
        | Let ({ Name = first }, Let (_, expr, _), Let ({ Name = second }, next, _))
                when first.StartsWith("first") && second.StartsWith("second") ->
            match expr with
            | LetTagNoChildrenWithProps tagInfo ->
                let newExpr = transformTagInfo tagInfo
                getChildren (newExpr :: currentList) next
            | TagNoChildren (tagName, range) ->
                let newExpr = transformTagInfo (TagInfo.NoChildren (tagName, [], range))
                getChildren (newExpr :: currentList) next
            | TagWithChildren callInfo ->
                let newExpr = transformTagInfo (TagInfo.WithChildren callInfo)
                getChildren (newExpr :: currentList) next
            | expr ->
                getChildren (expr :: currentList) next
        | _ ->
            currentList

    let listItemType =
        Type.Tuple (genericArgs = [ Type.String ; Type.Any ], isStruct = false)
    let emptyList =
        Value (kind = NewList (headAndTail = None, typ = listItemType), range = None)

    let convertExprListToExpr (exprs: Expr list) =
        (emptyList, exprs)
        ||> List.fold (fun acc prop ->
            Value (
                kind = NewList (headAndTail = Some (prop, acc), typ = listItemType),
                range = None
            )
        )

    let transformTagInfo (tagInfo: TagInfo) : Expr =
        let tagName, props, children, range =
            match tagInfo with
            | WithChildren (tagName, callInfo, range) ->
                let props = callInfo.Args |> List.fold getAttributes []
                let childrenList = callInfo.Args |> List.fold getChildren []
                tagName, props, childrenList, range
            | NoChildren (tagName, propList, range) ->
                let props = collectAttributes propList
                let childrenList = []
                tagName, props, childrenList, range

        let propsXs =
            props
            |> List.map (fun (name, expr) ->
                Value (
                    kind =
                        NewTuple (
                            values =
                                [
                                    // property name
                                    Value (kind = StringConstant name, range = None)
                                    // property value
                                    TypeCast (expr, Type.Any)
                                ],
                            isStruct = false
                        ),
                    range = None
                )
            )
        let childrenExpression = convertExprListToExpr children
        let childrenXs =
            Value (
                kind =
                    NewTuple (
                        values =
                            [
                                // property name
                                Value (kind = StringConstant "children", range = None)
                                // property value
                                TypeCast (childrenExpression, Type.Any)
                            ],
                        isStruct = false
                    ),
                range = None
            )
        let finalList = childrenXs :: propsXs
        let propsExpr = convertExprListToExpr finalList

        Call (
            callee = importJsxCreate,
            info =
                {
                    ThisArg = None
                    Args = [ Value(StringConstant tagName, None); propsExpr ]
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
        | LetTagNoChildrenWithProps tagCall ->
            transformTagInfo tagCall
        | TagNoChildren (tagName, range) ->
            transformTagInfo (TagInfo.NoChildren (tagName, [], range))
        | CallTagNoChildrenWithHandler tagCall ->
            transformTagInfo tagCall
        | TagWithChildren callInfo ->
            transformTagInfo (TagInfo.WithChildren callInfo)
        | Let (name, value, expr) ->
            Let (name, value, (transform expr))
        | Sequential expressions ->
            // transform only the last expression
            Sequential (expressions |> List.mapi (
                fun i expr ->
                    if i = expressions.Length - 1 then
                        transform expr
                    else
                        expr
            ))
        | _ ->
            expr


type SolidComponentAttribute() =
    inherit MemberDeclarationPluginAttribute()

    override _.FableMinimumVersion = "4.0"

    override this.Transform(compiler: PluginHelper, file: File, memberDecl: MemberDecl) =
        // Console.WriteLine("!Start! MemberDecl")
        // Console.WriteLine(memberDecl.Body)
        // Console.WriteLine("!End! MemberDecl")
        let newBody = AST.transform memberDecl.Body
        { memberDecl with Body = newBody }

    override _.TransformCall(_ph: PluginHelper, _mb: MemberFunctionOrValue, expr: Expr) : Expr = expr
