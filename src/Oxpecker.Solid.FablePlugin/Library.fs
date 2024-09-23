namespace Oxpecker.Solid.FablePlugin

open Fable
open Fable.AST
open Fable.AST.Fable

[<assembly: ScanForPlugins>]
do ()

module rec AST =
    type PropInfo = string * Expr
    type Props = PropInfo list

    let (|TagCall|_|) (expr: Expr) =
        match expr with
        | Call (Import (importInfo, _, _), callInfo, _, range) when importInfo.Selector.StartsWith("HtmlContainerExtensions_Run")  ->
            Some (callInfo, range)
        | _ -> None

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

    let rec collectProps (seqExpr: Expr list) =
        match seqExpr with
        | [] -> []
        | Sequential expressions :: rest ->
            collectProps rest @ collectProps expressions
        | Call (Import (importInfo, _, _), callInfo, _, range) :: rest ->
            match importInfo.Kind with
            | ImportKind.MemberImport (MemberRef(_, memberRefInfo)) when memberRefInfo.CompiledName.StartsWith("HtmlTag.set_") ->
                let propName =
                    // TODO: handle specific tag properties
                    match memberRefInfo.CompiledName.Substring("HtmlTag.set_".Length) with
                    | "class'" -> "class"
                    | "type'" -> "type"
                    | name -> name
                let propValue = callInfo.Args.Head
                [(propName, propValue)]
            | _ ->
                []
        | _ :: rest ->
            collectProps rest

    let getProps (callInfo: CallInfo) tagName : Props =
        let setPropertiesSeq = callInfo.Args |> List.tryPick (
            function
            | Let ({ Name = "returnVal" }, _, Sequential expr) -> Some expr
            | _ -> None
        )
        match setPropertiesSeq with
        | None -> []
        | Some seq -> collectProps seq

    let getChildren (callInfo: CallInfo) : Expr list =
        let setChildrenSeq = callInfo.Args |> List.choose (
            function
            | Let ({ Name = "element" }, TagCall (callInfo, range), _) -> Some (callInfo, range)
            | _ -> None
        )
        match setChildrenSeq with
        | [] -> []
        | callInfos ->
            [
                for callInfo, range in callInfos do
                    handleCallInfo callInfo range
            ]

    let getText (callInfo: CallInfo) : Expr list =
        let textSeq = callInfo.Args |> List.choose (
            function
            | Lambda ({ Name = "_arg" }, TypeCast (body, Unit) , None) ->
                Some body
            | _ -> None
        )
        textSeq

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

    let handleCallInfo (callInfo: CallInfo) range : Expr =
        let (DeclaredType (gType, _)) = callInfo.GenericArgs.Head
        let tagName = gType.FullName.Split('.') |> Seq.last
        let props = getProps callInfo tagName
        let childrenList = getChildren callInfo
        let textList = getText callInfo
        let childrenList = childrenList @ textList
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
        let childrenExpression = convertExprListToExpr childrenList
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
        | Let (name, value, expr) ->
            Let (name, value, (transform expr))
        | TagCall (callInfo, range) ->
            handleCallInfo callInfo range
        | _ ->
            expr


type JSXComputationExpressionAttribute() =
    inherit MemberDeclarationPluginAttribute()

    override _.FableMinimumVersion = "4.0"

    override this.Transform(compiler: PluginHelper, file: File, memberDecl: MemberDecl) =
        // Console.WriteLine("!Start! MemberDecl");
        // Console.WriteLine(memberDecl.Body)
        // Console.WriteLine("!End! MemberDecl");
        let newBody = AST.transform memberDecl.Body
        { memberDecl with Body = newBody }

    override _.TransformCall(_ph: PluginHelper, _mb: MemberFunctionOrValue, expr: Expr) : Expr = expr
