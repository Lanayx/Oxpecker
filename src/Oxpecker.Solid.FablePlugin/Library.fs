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

    type TagCall =
        | WithChildren of tagName:string * propsAndChildren:CallInfo * range:SourceLocation option
        | NoChildren of tagName:string * props:Expr list * range:SourceLocation option

    let (|SingleTag|_|) (expr: Expr) =
        match expr with
        | Call (Import (importInfo, LambdaType (_, DeclaredType (typ, [])), _), _, _, range)
                when importInfo.Selector.EndsWith("_$ctor")
                && typ.FullName.StartsWith("Oxpecker.Solid") ->
            let tagName = typ.FullName.Split('.') |> Seq.last
            let finalTagName = if tagName = "__" then "" else tagName
            Some (finalTagName, range)
        | _ ->
            None

    let (|RegularTag|_|) (expr: Expr) =
        match expr with
        | Call (Import (importInfo, _, _), callInfo, _, range)
                when importInfo.Selector.StartsWith("HtmlContainerExtensions_Run")  ->
            match callInfo.Args.Head with
            | SingleTag (tagName, _)->
                Some (tagName, callInfo, range)
            | Call (_, innerCallInfo, _, _) ->
                match innerCallInfo.Args.Head with
                | SingleTag (tagName, _) ->
                    Some (tagName, callInfo, range)
                | _ ->
                    None
            | Let ({ Name = name }, SingleTag (tagName, _), _)
                    when name.StartsWith("returnVal") ->
                Some (tagName, callInfo, range)
            | _ -> None
        | _ -> None

    let (|LetElement|_|) =
        function
        | Let ({ Name = name }, _, _) when name.StartsWith("element") ->
            Some ()
        | _ ->
            None

    let (|LetRegularTag|_|) =
        function
        | Let (_, RegularTag (tagName, callInfo, range), _) ->
            TagCall.WithChildren (tagName, callInfo, range) |> Some
        | _ ->
            None

    let (|LetSingleTagWithProps|_|) =
        function
        | Let (_, SingleTag (tagName, range), Sequential exprs) ->
            TagCall.NoChildren (tagName, exprs, range) |> Some
        | _ ->
            None

    let (|CallSingleTagWithHandler|_|) (expr: Expr) =
        match expr with
        | Call (Import (importInfo, _, _), { Args = SingleTag (tagName, _) :: _ }, _, range)
                when importInfo.Selector.StartsWith("HtmlElementExtensions_on") ->
            TagCall.NoChildren (tagName, [expr], range) |> Some
        | _ ->
            None


    let (|LetSingleTagNoProps|_|) =
        function
        | Let (_, SingleTag (tagName, range), _) ->
            TagCall.NoChildren (tagName, [], range) |> Some
        | _ ->
            None

    let (|SimpleText|_|) =
        function
        | Lambda ({ Name = txt }, TypeCast (textBody, Unit), None)
            when txt.StartsWith("txt") ->
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

    let rec collectAttributes (seqExpr: Expr list) =
        match seqExpr with
        | [] -> []
        | Sequential expressions :: rest ->
            collectAttributes rest @ collectAttributes expressions
        | Call (Import (importInfo, _, _), { Args = [_ ; Value (StringConstant eventName, _) ; handler] }, _, _) :: _ ->
            match importInfo.Kind with
            | ImportKind.MemberImport (MemberRef(entity, memberRefInfo))  when
                    entity.FullName.StartsWith("Oxpecker.Solid") ->
                if memberRefInfo.CompiledName = "on" then
                    [("on:" + eventName, handler)]
                else
                    []
            | _ ->
                []
        | Call (Import (importInfo, _, _), callInfo, _, _) :: _ ->
            match importInfo.Kind with
            | ImportKind.MemberImport (MemberRef(entity, memberRefInfo)) when
                    entity.FullName.StartsWith("Oxpecker.Solid") ->
                let setterIndex = memberRefInfo.CompiledName.IndexOf("set_")
                if setterIndex >= 0 then
                    let propName =
                        match memberRefInfo.CompiledName.Substring(setterIndex + "set_".Length) with
                        | "class'" -> "class"
                        | "type'" -> "type"
                        | name -> name
                    let propValue = callInfo.Args.Head
                    [(propName, propValue)]
                else
                    []
            | _ ->
                []
        | _ :: rest ->
            collectAttributes rest

    let getAttributes currentList (expr: Expr): Props =
        match expr with
        | Let ({ Name = returnVal }, _, Sequential exprs)
                when returnVal.StartsWith("returnVal") ->
            collectAttributes exprs @ currentList
        | Call (Import (importInfo, _, _), _, _, _)
                when importInfo.Selector.StartsWith("HtmlElementExtensions_on") ->
            collectAttributes [expr] @ currentList
        | _ ->
            currentList

    let getChildren currentList (expr: Expr): Expr list =
        match expr with
        | LetElement & LetRegularTag tagCall ->
           let newExpr = handleTagCall tagCall
           newExpr :: currentList
        | LetElement & Let (_, LetSingleTagWithProps tagCall, _)  ->
           let newExpr = handleTagCall tagCall
           newExpr :: currentList
        | LetElement & LetSingleTagNoProps tagCall ->
           let newExpr = handleTagCall tagCall
           newExpr :: currentList
        | LetElement & Let (_, CallSingleTagWithHandler tagCall, _) ->
           let newExpr = handleTagCall tagCall
           newExpr :: currentList
        | SimpleText body ->
            body :: currentList
        // text then tag
        | Let ({ Name = second }, next, Lambda ({ Name = builder },
                Sequential (TypeCast (textBody, Unit)::_), _))
                when second.StartsWith("second") && builder.StartsWith("builder") ->
            getChildren (textBody ::currentList) next
        // tag then text
        | Let ({ Name = first }, next, Lambda ({ Name = builder }, Sequential [
                    CurriedApply _; CurriedApply (Lambda ({ Name = txt }, TypeCast (textBody, Unit), Some second), _, _, _)
                ], _))
                when first.StartsWith("first") && builder.StartsWith("builder") && txt.StartsWith("txt") && second.StartsWith("second") ->
            textBody :: (getChildren currentList next)
        | Let ({ Name = first }, LetElement & Let (_, expr, _), Let ({ Name = second }, next, _))
                when first.StartsWith("first") && second.StartsWith("second") ->
            match expr with
            | LetSingleTagWithProps tagCall ->
                let newExpr = handleTagCall tagCall
                getChildren (newExpr :: currentList) next
            | SingleTag (tagName, range) ->
                let newExpr = handleTagCall (TagCall.NoChildren (tagName, [], range))
                getChildren (newExpr :: currentList) next
            | RegularTag callInfo ->
                let newExpr = handleTagCall (TagCall.WithChildren callInfo)
                getChildren (newExpr :: currentList) next
            | expr ->
                //Console.WriteLine(expr)
                currentList
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

    let handleTagCall (tagCall: TagCall) : Expr =
        let tagName, props, children, range =
            match tagCall with
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
        | LetSingleTagWithProps tagCall ->
            handleTagCall tagCall
        | SingleTag (tagName, range) ->
            handleTagCall (TagCall.NoChildren (tagName, [], range))
        | CallSingleTagWithHandler tagCall ->
            handleTagCall tagCall
        | RegularTag callInfo ->
            handleTagCall (TagCall.WithChildren callInfo)
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
        Console.WriteLine("!Start! MemberDecl")
        Console.WriteLine(memberDecl.Body)
        Console.WriteLine("!End! MemberDecl")
        let newBody = AST.transform memberDecl.Body
        { memberDecl with Body = newBody }

    override _.TransformCall(_ph: PluginHelper, _mb: MemberFunctionOrValue, expr: Expr) : Expr = expr
