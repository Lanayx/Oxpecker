namespace Oxpecker.Solid

open System
open Fable
open Fable.AST
open Fable.AST.Fable
open Oxpecker.Solid.FablePlugin
open FablePlugin.Types

[<assembly: ScanForPlugins>]
do () // Prompts fable to utilise this plugin

[<AutoOpen>]
module TraceSettings =
    let mutable private _verbose = false
    let enableTrace () = _verbose <- true
    let verbose () = _verbose
    type Tracer<'T> with
        member this.emit
            (
                message: string,
                memberName: string,
                path: string,
                line: int,
                [<Runtime.InteropServices.Optional; Runtime.InteropServices.DefaultParameterValue(true)>] emitJson: bool
            ) =
            if verbose() |> not then
                this
            else
                Console.ForegroundColor <- this.ConsoleColor
                Console.Write $"{this.Guid} "
                Console.ForegroundColor <- ConsoleColor.Gray
                Console.Write $"{memberName, -20}"
                Console.ResetColor()
                Console.WriteLine $"{message} ({path}:{line})"
                if PluginConfiguration.Jsonify() && emitJson then
                    Console.WriteLine $"{PrettyPrinter.print this.Value}"
                this
        member this.trace
            (
                [<Runtime.InteropServices.Optional; Runtime.InteropServices.DefaultParameterValue("")>] message: string,
                [<Runtime.InteropServices.Optional;
                  Runtime.CompilerServices.CallerMemberName;
                  Runtime.InteropServices.DefaultParameterValue("")>] memberName: string,
                [<Runtime.CompilerServices.CallerFilePath;
                  Runtime.InteropServices.Optional;
                  Runtime.InteropServices.DefaultParameterValue("")>] path: string,
                [<Runtime.CompilerServices.CallerLineNumber;
                  Runtime.InteropServices.Optional;
                  Runtime.InteropServices.DefaultParameterValue(0)>] line: int
            ) =
            this.emit(message, memberName, path, line)
        member this.ping
            (
                [<Runtime.InteropServices.Optional; Runtime.InteropServices.DefaultParameterValue("")>] message: string,
                [<Runtime.InteropServices.Optional;
                  Runtime.CompilerServices.CallerMemberName;
                  Runtime.InteropServices.DefaultParameterValue("")>] memberName: string,
                [<Runtime.CompilerServices.CallerFilePath;
                  Runtime.InteropServices.Optional;
                  Runtime.InteropServices.DefaultParameterValue("")>] path: string,
                [<Runtime.CompilerServices.CallerLineNumber;
                  Runtime.InteropServices.Optional;
                  Runtime.InteropServices.DefaultParameterValue(0)>] line: int
            ) =
            this.emit(message, memberName, path, line) |> ignore
        static member ping
            (
                [<Runtime.InteropServices.Optional; Runtime.InteropServices.DefaultParameterValue("")>] message: string,
                [<Runtime.InteropServices.Optional;
                  Runtime.CompilerServices.CallerMemberName;
                  Runtime.InteropServices.DefaultParameterValue("")>] memberName: string,
                [<Runtime.CompilerServices.CallerFilePath;
                  Runtime.InteropServices.Optional;
                  Runtime.InteropServices.DefaultParameterValue("")>] path: string,
                [<Runtime.CompilerServices.CallerLineNumber;
                  Runtime.InteropServices.Optional;
                  Runtime.InteropServices.DefaultParameterValue(0)>] line: int
            ) =
            if verbose() then
                Console.Write "                                     "
                Console.ForegroundColor <- ConsoleColor.Gray
                Console.Write $"{memberName, -20}"
                Console.ResetColor()
                Console.WriteLine $"{message} ({path}:{line})"
module internal rec AST =
    module Tracer =
        let private _random = Random()
        let inline private _create value guid consoleColor = {
            Value = value
            Guid = guid
            ConsoleColor = consoleColor
        }
        let create value =
            if verbose() |> not then
                _create value Guid.Empty ConsoleColor.Black
            else
                _random.Next(15) |> enum<ConsoleColor> |> _create value (Guid.NewGuid())
        let init = _create () Guid.Empty ConsoleColor.Black
        let map (tracer: Tracer<'T>) (input: 'M) : Tracer<'M> =
            _create input tracer.Guid tracer.ConsoleColor
        let (|Traced|) (tracer: Tracer<'T>) = (tracer.Value, tracer)
        let trace
            (
                [<Runtime.InteropServices.Optional; Runtime.InteropServices.DefaultParameterValue("")>] message: string,
                [<Runtime.InteropServices.Optional;
                  Runtime.CompilerServices.CallerMemberName;
                  Runtime.InteropServices.DefaultParameterValue("")>] memberName: string,
                [<Runtime.CompilerServices.CallerFilePath;
                  Runtime.InteropServices.Optional;
                  Runtime.InteropServices.DefaultParameterValue("")>] path: string,
                [<Runtime.CompilerServices.CallerLineNumber;
                  Runtime.InteropServices.Optional;
                  Runtime.InteropServices.DefaultParameterValue(0)>] line: int
            )
            (tracer: Tracer<'a>)
            =
            tracer.emit(message, memberName, path, line)

    [<AutoOpen>]
    module Native =
        let (|StartsWith|_|) (value: string) : string -> unit option =
            function
            | s when s.StartsWith(value) ->
                Tracer.ping($"Starts with {value}")
                Some()
            | _ -> None
        let (|EndsWith|_|) (value: string) : string -> unit option =
            function
            | s when s.EndsWith(value) -> Some()
            | _ -> None
        let (|Equals|_|) (value: string) : string -> unit option =
            function
            | s when s.Equals(value) ->
                Tracer.ping($"Equals {value}")
                Some()
            | _ -> None
        let (|Trim|) (value: int) (input: string) =
            input.Substring(0, input.Length - value)
    [<RequireQualifiedAccess>]
    module EntityRef =
        let (|StartsWith|_|) (value: string) =
            function
            | e when e.FullName.StartsWith value ->
                Tracer.ping($"EntityRef Startswith {value}")
                Some()
            | _ -> None
    [<RequireQualifiedAccess>]
    module Ident =
        let (|StartsWith|_|) (value: string) : Ident -> unit option =
            function
            | i when i.Name.StartsWith value ->
                Tracer.ping($"Ident starts with {value}")
                Some()
            | _ -> None
    [<RequireQualifiedAccess>]
    module ImportInfo =
        let (|StartsWith|_|) (value: string) =
            function
            | i when i.Selector.StartsWith value ->
                Tracer.ping($"ImportInfo selector startswith {value}")
                Some()
            | _ -> None
        let (|Equals|_|) (value: string) =
            function
            | i when i.Selector = value ->
                Tracer.ping($"ImportInfo selector is equal to {value}")
                Some()
            | _ -> None
    [<RequireQualifiedAccess>]
    module Import =
        let (|Info|_|) =
            function
            | Import(value, _, _) -> Some value
            | _ -> None
        let (|Type|_|) =
            function
            | Import(_, value, _) -> Some value
            | _ -> None
        let (|Range|_|) =
            function
            | Import(_, _, value) -> Some value
            | _ -> None
    [<RequireQualifiedAccess>]
    module Call =
        /// <summary>
        /// Matches expressions for tags that are imported from a namespace starting with <c>Oxpecker.Solid</c>
        /// </summary>
        /// <returns><c>Expr * SourceLocation</c></returns>
        let (|ImportTag|_|) (expr: Expr) =
            match expr with
            | Call(Import({ Kind = UserImport false }, Any, _) as imp,
                   CallInfo.GetTags [ "new" ],
                   DeclaredType(EntityRef.StartsWith "Oxpecker.Solid", []),
                   range) ->
                Tracer.ping()
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
            | Call(Import(importInfo, LambdaType(_, DeclaredType(typ, _)), _), callInfo, _, range) when
                condition importInfo
                ->
                let tagImport =
                    match callInfo.Args with
                    | Call.ImportTag(imp, _) :: _
                    | Let(_, Call.ImportTag(imp, _), _) :: _ ->
                        Tracer.ping(
                            $"CallInfo.Args is Let(_, Call.ImportTag(imp, _), _) :: _ or Call.ImportTag(imp,_)::_"
                        )
                        LibraryImport imp
                    | _ ->
                        Tracer.ping("AutoImport")
                        let tagName = typ.FullName.Split('.') |> Seq.last
                        match tagName with
                        | "Fragment" -> ""
                        | EndsWith "'" & Trim (1) s -> s
                        | EndsWith "`1" & Trim (2) s -> s
                        | _ -> tagName
                        |> AutoImport
                Some(tagImport, callInfo, range)
            | _ -> None
    [<RequireQualifiedAccess>]
    module CallInfo =
        let (|GetArgs|) = _.Args
        let (|GetTags|) = _.Tags
    [<RequireQualifiedAccess>]
    module Tag =
        /// <summary>
        /// Pattern matches expressions to Tags calls without children
        /// </summary>
        /// <returns><c>TagInfo.NoChildren</c></returns>
        let (|NoChildren|_|) (expr: Expr) =
            let condition = _.Selector.EndsWith("_$ctor")
            match expr with
            | Call.Tag condition (tagName, _, range) ->
                Tracer.ping()
                Some(tagName, range)
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
            | Call.Tag condition tagCallInfo ->
                Tracer.ping()
                Some tagCallInfo
            | _ -> None
    [<RequireQualifiedAccess>]
    module Let =
        let (|Ident|_|) =
            function
            | Let(value, _, _) -> Some value
            | _ -> None
        let (|Value|_|) =
            function
            | Let(_, value, _) -> Some value
            | _ -> None
        let (|Body|_|) =
            function
            | Let(_, _, value) -> Some value
            | _ -> None
        /// <summary>
        /// Pattern matches <c>let</c> bindings that start with <c>element</c>
        /// </summary>
        /// <returns><c>unit</c></returns>
        let (|Element|_|) =
            function
            | Let.Ident(Ident.StartsWith "element") ->
                Tracer.ping()
                Some()
            | _ -> None
        /// <summary>
        /// Pattern matches <c>let</c> bindings for Tags with children
        /// </summary>
        /// <returns><c>TagInfo.WithChildren</c></returns>
        let (|TagWithChildren|_|) =
            function
            | Let.Value(Tag.WithChildren(tagName, callInfo, range)) ->
                Tracer.ping()
                TagInfo.WithChildren(tagName, callInfo, range) |> Some
            | _ -> None
        /// <summary>
        /// Pattern matches <c>let</c> bindings for Tags without children (but with props)
        /// </summary>
        /// <returns><c>TagInfo.NoChildren</c></returns>
        let (|TagNoChildrenWithProps|_|) =
            function
            | Let(_, Tag.NoChildren(tagName, range), Sequential exprs) ->
                Tracer.ping()
                TagInfo.NoChildren(tagName, exprs, range) |> Some
            | _ -> None
    /// <summary>
    /// Pattern matches expressions (<c>let</c> or otherwise) for tags without children directly to Tag calls
    /// </summary>
    /// <returns><c>TagInfo.NoChildren</c></returns>
    let (|CallTagNoChildrenWithHandler|_|) (expr: Expr) =
        match expr with
        | Call(Import.Info(ImportInfo.StartsWith "HtmlElementExtensions_"), CallInfo.GetArgs(args :: _), _, range) ->
            Tracer.ping(
                $"Meets Condition 1: Call(Import.Info(ImportInfo.StartsWith 'HtmlElementExtensions_'), CallInfo.GetArgs(args :: _), _, range)"
            )
            (args, range)
            |> function
                | Tag.NoChildren(tagName, _), range ->
                    Tracer.ping("Condition 2 Tag.NoChildren(tagName, _), range ")
                    TagInfo.NoChildren(tagName, [ expr ], range) |> Some
                | Let.TagNoChildrenWithProps(NoChildren(tagName, props, _)), range ->
                    Tracer.ping("Condition 2 Let.TagNoChildrenWithProps(NoChildren(tagName, props, _)), range")
                    TagInfo.NoChildren(tagName, expr :: props, range) |> Some
                | CallTagNoChildrenWithHandler(NoChildren(tagName, props, _)), range ->
                    Tracer.ping("Condition 2 CallTagNoChildrenWithHandler(NoChildren(tagName, props, _)), range")
                    TagInfo.NoChildren(tagName, expr :: props, range) |> Some
                | _ -> None
        | _ -> None
    /// <summary>
    /// Pattern matches <c>let</c> bindings for tags without children or props
    /// </summary>
    /// <returns><c>TagInfo.NoChildren</c></returns>
    let (|LetTagNoChildrenNoProps|_|) =
        function
        | Let.Value(Tag.NoChildren(tagName, range)) ->
            Tracer.ping()
            TagInfo.NoChildren(tagName, [], range) |> Some
        | _ -> None
    /// <summary>
    /// Pattern matches expressions that are text in isolation (no siblings)
    /// </summary>
    /// <returns><c>Expr</c> of text</returns>
    let (|TextNoSiblings|_|) =
        function
        | Lambda(Ident.StartsWith "cont", TypeCast(textBody, Unit), None) ->
            Tracer.ping()
            Some textBody
        | _ -> None

    let private (|EventHandler|_|) callInfo =
        match callInfo with
        | CallInfo.GetArgs [ _; Value(StringConstant eventName, _); handler ] ->
            Tracer.ping()
            Some(eventName, handler)
        | _ -> None
    [<AutoOpen>]
    module Attributes =
        let (|Extension|_|) =
            function
            | "on", EventHandler(eventName, handler), restResults ->
                Tracer.ping("on")
                ("on:" + eventName, handler) :: restResults |> Some
            | "bool", EventHandler(eventName, handler), restResults ->
                Tracer.ping("bool")
                ("bool:" + eventName, handler) :: restResults |> Some
            | "data", EventHandler(eventName, handler), restResults ->
                Tracer.ping("data")
                ("data-" + eventName, handler) :: restResults |> Some
            | "attr", EventHandler(eventName, handler), restResults ->
                Tracer.ping("attr")
                (eventName, handler) :: restResults |> Some
            | "ref", CallInfo.GetArgs [ _; identExpr ], restResults ->
                Tracer.ping("ref")
                ("ref", identExpr) :: restResults |> Some
            | "style'", CallInfo.GetArgs [ _; identExpr ], restResults ->
                Tracer.ping("style")
                ("style", identExpr) :: restResults |> Some
            | "classList", CallInfo.GetArgs [ _; identExpr ], restResults ->
                Tracer.ping("classList")
                ("classList", identExpr) :: restResults |> Some
            | _ -> None
        let rec collectAttributes (tracer: Expr list Tracer) =
            let exprs = tracer.Value
            match exprs with
            | [] -> []
            | Sequential expressions :: rest ->
                Tracer.ping("| [] -> [] | Sequential expressions :: rest")
                (Tracer.map tracer >> collectAttributes) rest
                @ (Tracer.map tracer >> collectAttributes) expressions
            | Call(Import.Info importInfo, callInfo, _, _) :: rest ->
                Tracer.ping("Condition 1: Call(Import.Info importInfo, callInfo, _, _) :: rest")
                let restResults = (Tracer.map tracer >> collectAttributes) rest
                match importInfo.Kind with
                | ImportKind.MemberImport(MemberRef(EntityRef.StartsWith "Oxpecker.Solid", memberRefInfo)) ->
                    Tracer.ping
                        $"Condition 2: ImportKind.MemberImport(MemberRef(EntityRef.StartsWith 'Oxpecker.Solid', memberRefInfo))"
                    match memberRefInfo.CompiledName, callInfo, restResults with
                    | Extension propList ->
                        Tracer.ping($"Condition 3: Extension propList ")
                        propList
                    | _ ->
                        Tracer.ping($"Condition 3: _")
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
                                                    _)) ->
                                Tracer.ping
                                    $"Condition 4: TypeCast(expr, DeclaredType({{ FullName = 'Oxpecker.Solid.Builder.HtmlElement' }},_))"
                                (propName, transform expr) :: restResults
                            | Delegate(args, expr, name, tags) ->
                                Tracer.ping($"Condition 4: Delegate(args, expr, name, tags)")
                                (propName, Delegate(args, transform expr, name, tags)) :: restResults
                            | _ ->
                                Tracer.ping($"Condition 4: _")
                                (propName, propValue) :: restResults
                        else
                            Tracer.ping($"Setter index was >= 0")
                            restResults
                | _ ->
                    Tracer.ping($"Condition 2: _")
                    restResults
            | Set(IdentExpr(Ident.StartsWith "returnVal"), SetKind.FieldSet name, _, handler, _) :: rest ->
                Tracer.ping(
                    $"Condition 1: Set(IdentExpr(Ident.StartsWith \"returnVal\"), SetKind.FieldSet name, _, handler, _) :: rest"
                )
                let propName =
                    match name with
                    | name when name.EndsWith("'") -> name.Substring(0, name.Length - 1) // like class' or type'
                    | name -> name
                (propName, handler) :: (Tracer.map tracer >> collectAttributes) rest
            | _ :: rest ->
                Tracer.ping($"Condition 1: _ :: rest")
                (Tracer.map tracer >> collectAttributes) rest

        let getAttributes currentList (tracer: Expr Tracer) : Props =
            tracer.ping()
            let expr = tracer.Value
            match expr with
            | Let(Ident.StartsWith "returnVal", _, Sequential exprs) ->
                Tracer.ping("Let(Ident.StartsWith \"returnVal\", _, Sequential exprs)")
                (Tracer.map(tracer.trace()) >> collectAttributes) exprs @ currentList
            | CallTagNoChildrenWithHandler(NoChildren(_, props, _)) ->
                Tracer.ping("CallTagNoChildrenWithHandler(NoChildren(_, props, _))")
                (Tracer.map(tracer.trace()) >> collectAttributes) props @ currentList
            | _ ->
                Tracer.ping("_")
                currentList
    [<AutoOpen>]
    module Children =
        let getChildren currentList (tracer: Expr Tracer) : Expr list =
            let expr = tracer.trace().Value
            match expr with
            | Let.TagWithChildren tagInfo ->
                Tracer.ping("Let.TagWithChildren tagInfo")
                let newExpr = transformTagInfo(tagInfo |> Tracer.create)
                newExpr :: currentList
            | Let.Element & Let.Value(Let.TagNoChildrenWithProps tagInfo) ->
                Tracer.ping("Let.Element & Let.Value(Let.TagNoChildrenWithProps tagInfo)")
                let newExpr = transformTagInfo(tagInfo |> Tracer.create)
                newExpr :: currentList
            | Let.Element & LetTagNoChildrenNoProps tagInfo ->
                Tracer.ping("Let.Element & LetTagNoChildrenNoProps tagInfo")
                let newExpr = transformTagInfo(tagInfo |> Tracer.create)
                newExpr :: currentList
            | Let.Element & Let.Value(CallTagNoChildrenWithHandler tagInfo) ->
                Tracer.ping("Let.Element & Let.Value(CallTagNoChildrenWithHandler tagInfo)")
                let newExpr = transformTagInfo(tagInfo |> Tracer.create)
                newExpr :: currentList
            | Let.Element & Let.Value next ->
                Tracer.ping("Let.Element & Let.Value next")
                let newExpr = transform next
                newExpr :: currentList
            // Lambda with two arguments returning element
            | Lambda(Ident.StartsWith "cont", TypeCast(Lambda(item, Lambda(index, next, _), _), _), _) ->
                Tracer.ping(
                    "Lambda(Ident.StartsWith \"cont\", TypeCast(Lambda(item, Lambda(index, next, _), _), _), _)"
                )
                Delegate([ item; index ], transform next, None, []) :: currentList
            | TextNoSiblings body ->
                Tracer.ping "TextNoSiblings body"
                body :: currentList
            // text with solid signals inside
            | Let(Ident.StartsWith "text", body, TextNoSiblings _) ->
                Tracer.ping "Let(Ident.StartsWith \"text\", body, TextNoSiblings _)"
                body :: currentList
            // text then tag
            | Let(Ident.StartsWith "second",
                  next,
                  Lambda(Ident.StartsWith "builder", Sequential(TypeCast(textBody, Unit) :: _), _)) ->
                Tracer.ping
                    "Let(Ident.StartsWith \"second\", next, Lambda(Ident.StartsWith \"builder\", Sequential(TypeCast(textBody, Unit) :: _), _))"
                getChildren (textBody :: currentList) (Tracer.map tracer next)
            // parameter then another parameter
            | CurriedApply(Lambda(Ident.StartsWith "cont", TypeCast(lastParameter, Unit), Some(StartsWith "second")),
                           _,
                           _,
                           _) ->
                Tracer.ping
                    "CurriedApply(Lambda(Ident.StartsWith \"cont\", TypeCast(lastParameter, Unit), Some(StartsWith \"second\")), _, _, _)"
                lastParameter :: currentList
            | CurriedApply(Lambda(Ident.StartsWith "builder", Sequential [ TypeCast(middleParameter, Unit); next ], _),
                           _,
                           _,
                           _)
            | Lambda(Ident.StartsWith "builder", Sequential [ TypeCast(middleParameter, Unit); next ], _) ->
                Tracer.ping
                    "| CurriedApply(Lambda(Ident.StartsWith \"builder\", Sequential [ TypeCast(middleParameter, Unit); next ], _), _, _, _) | Lambda(Ident.StartsWith \"builder\", Sequential [ TypeCast(middleParameter, Unit); next ], _)"
                getChildren (middleParameter :: currentList) (Tracer.map tracer next)
            // tag then text
            | Let(Ident.StartsWith "first",
                  next1,
                  Lambda(Ident.StartsWith "builder", Sequential [ CurriedApply _; next2 ], _)) ->
                Tracer.ping
                    "Let(Ident.StartsWith \"first\", next1, Lambda(Ident.StartsWith \"builder\", Sequential [ CurriedApply _; next2 ], _))"
                let next2Children = getChildren [] (Tracer.map tracer next2)
                let next1Children = getChildren [] (Tracer.map tracer next1)
                next2Children @ next1Children @ currentList
            | Let(Ident.StartsWith "first", Let.Value expr, Let(Ident.StartsWith "second", next, _))
            | Let(Ident.StartsWith "first", expr, Let(Ident.StartsWith "second", next, _)) ->
                Tracer.ping
                    "Condition 1: | Let(Ident.StartsWith \"first\", Let.Value expr, Let(Ident.StartsWith \"second\", next, _)) | Let(Ident.StartsWith \"first\", expr, Let(Ident.StartsWith \"second\", next, _))"
                match expr with
                | Let.TagNoChildrenWithProps tagInfo ->
                    Tracer.ping("Condition 2: Let.TagNoChildrenWithProps tagInfo")
                    let newExpr = transformTagInfo(tagInfo |> Tracer.create)
                    getChildren (newExpr :: currentList) (Tracer.map tracer next)
                | Tag.NoChildren(tagName, range) ->
                    Tracer.ping("Condition 2: Tag.NoChildren(tagName, range) ")
                    let newExpr =
                        transformTagInfo(TagInfo.NoChildren(tagName, [], range) |> Tracer.create)
                    getChildren (newExpr :: currentList) (Tracer.map tracer next)
                | Tag.WithChildren callInfo ->
                    Tracer.ping("Condition 2: Tag.WithChildren callInfo")
                    let newExpr = transformTagInfo(TagInfo.WithChildren callInfo |> Tracer.create)
                    getChildren (newExpr :: currentList) (Tracer.map tracer next)
                | expr ->
                    Tracer.ping("Condition 2: expr")
                    let newExpr = transform expr
                    getChildren (newExpr :: currentList) (Tracer.map tracer next)
            | IfThenElse(guardExpr, thenExpr, elseExpr, range) ->
                Tracer.ping("IfThenElse(guardExpr, thenExpr, elseExpr, range)")
                IfThenElse(guardExpr, transform thenExpr, transform elseExpr, range)
                :: currentList
            | DecisionTree(decisionTree, targets) ->
                Tracer.ping("DecisionTree(decisionTree, targets)")
                DecisionTree(decisionTree, targets |> List.map(fun (target, expr) -> target, transform expr))
                :: currentList
            // router cases
            | Call(Get(IdentExpr _, FieldGet _, Any, _), { Args = args }, _, _) ->
                Tracer.ping("Condition 1: Call(Get(IdentExpr _, FieldGet _, Any, _), { Args = args }, _, _)")
                match args with
                | [ Call(Import(ImportInfo.Equals "uncurry2", Any, None), { Args = [ Lambda(_, body, _) ] }, _, _) ] ->
                    Tracer.ping(
                        "Condition 2: [ Call(Import(ImportInfo.Equals \"uncurry2\", Any, None), { Args = [ Lambda(_, body, _) ] }, _, _) ]"
                    )
                    getChildren currentList (Tracer.map tracer body)
                | [ Call.ImportTag(imp, _) ] ->
                    Tracer.ping("Condition 2: [ Call.ImportTag(imp, _) ]")
                    let newExpr =
                        transformTagInfo(TagInfo.NoChildren(LibraryImport imp, [], None) |> Tracer.create)
                    newExpr :: currentList
                | [ Let(_, Call.ImportTag(imp, _), Sequential exprs) ] ->
                    Tracer.ping("Condition 2: [ Let(_, Call.ImportTag(imp, _), Sequential exprs) ]")
                    let newExpr =
                        transformTagInfo(TagInfo.NoChildren(LibraryImport imp, exprs, None) |> Tracer.create)
                    newExpr :: currentList
                | [ Let(_, Let(_, Call.ImportTag(imp, _), Sequential exprs), Tag.WithChildren(_, callInfo, _)) ] ->
                    Tracer.ping(
                        "Condition 2: [ Let(_, Let(_, Call.ImportTag(imp, _), Sequential exprs), Tag.WithChildren(_, callInfo, _)) ]"
                    )
                    let newExpr =
                        transformTagInfo(TagInfo.Combined(LibraryImport imp, exprs, callInfo, None) |> Tracer.create)
                    newExpr :: currentList
                | [ next1; next2 ] ->
                    Tracer.ping("Condition 2: [ next1; next2 ]")
                    let next2Children = getChildren [] (Tracer.map tracer next2)
                    let next1Children = getChildren [] (Tracer.map tracer next1)
                    next2Children @ next1Children @ currentList
                | [ expr ] ->
                    Tracer.ping("Condition 2: [ expr ]")
                    expr :: currentList
                | _ ->
                    Tracer.ping("Condition 2: _")
                    currentList
            | Let.Ident {
                            Name = name
                            Range = range
                            Type = DeclaredType({ FullName = fullName }, [])
                        } when
                ((name.StartsWith("returnVal") && fullName.StartsWith("Oxpecker.Solid"))
                 || (name.StartsWith("element") && fullName.StartsWith("Oxpecker.Solid")))
                |> not
                ->
                Tracer.ping
                    "Let.Ident { Name = name; Range = range; Type = DeclaredType({ FullName = fullName }, []) } when ((name.StartsWith(\"returnVal\") && fullName.StartsWith(\"Oxpecker.Solid\")) || (name.StartsWith(\"element\") && fullName.StartsWith(\"Oxpecker.Solid\"))) |> not"
                match range with
                | Some range ->
                    failwith $"`let` binding inside HTML CE can't be converted to JSX:line {range.start.line}"
                | None -> failwith $"`let` binding inside HTML CE can't be converted to JSX"
            | _ -> currentList
    [<RequireQualifiedAccess>]
    module Baked =
        let listItemType =
            Type.Tuple(genericArgs = [ Type.String; Type.Any ], isStruct = false)
        let emptyList =
            Value(kind = NewList(headAndTail = None, typ = listItemType), range = None)

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


    let convertExprListToExpr (exprs: Expr list) =
        Tracer.ping()
        (Baked.emptyList, exprs)
        ||> List.fold(fun acc prop ->
            Value(kind = NewList(headAndTail = Some(prop, acc), typ = Baked.listItemType), range = None))

    let wrapChildrenExpression childrenExpression =
        Tracer.ping()
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

    let transformTagInfo (tracer: TagInfo Tracer) : Expr =
        tracer.ping()
        let tagInfo = tracer.Value
        let tagName, props, children, range =
            match tagInfo with
            | WithChildren(tagName, callInfo, range) ->
                Tracer.ping("tagInfo - WithChildren(tagName, callInfo, range)")
                let props =
                    callInfo.Args |> List.map(Tracer.map tracer) |> List.fold getAttributes []
                let childrenList =
                    callInfo.Args |> List.map(Tracer.map tracer) |> List.fold getChildren []
                tagName, props, childrenList, range
            | NoChildren(tagName, propList, range) ->
                Tracer.ping("tagInfo - NoChildren(tagName, propLiust, range")
                let props = propList |> Tracer.map tracer |> collectAttributes
                let childrenList = []
                tagName, props, childrenList, range
            | Combined(tagName, propList, callInfo, range) ->
                Tracer.ping("tagInfo - Combined(tagName, propList, callInfo, range")
                let props = propList |> Tracer.map tracer |> collectAttributes
                let childrenList =
                    callInfo.Args |> List.map(Tracer.map tracer) |> List.fold getChildren []
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
            | AutoImport tagName ->
                Tracer.ping("tag - AutoImport")
                Value(StringConstant tagName, None)
            | LibraryImport imp ->
                Tracer.ping("tag - LibraryImport")
                imp
        tracer.ping("Finalised")
        Call(
            callee = Baked.importJsxCreate,
            info = {
                ThisArg = None
                Args = [ tag; propsExpr ]
                SignatureArgTypes = []
                GenericArgs = []
                MemberRef = None
                Tags = [ "jsx" ]
            },
            typ = Baked.jsxElementType,
            range = range
        )

    let transform' (tracer: Expr Tracer) =
        tracer.ping()
        let expr = tracer.Value
        match expr with
        | Let.TagNoChildrenWithProps tagCall
        | Let.Element & Let(_, Let.TagNoChildrenWithProps tagCall, _) ->
            transformTagInfo(
                tagCall
                |> Tracer.create
                |> _.trace(
                    "| Let.TagNoChildrenWithProps tagCall | Let.Element & Let(_, Let.TagNoChildrenWithProps tagCall, _)"
                )
            )
        | Tag.NoChildren(tagName, range) ->
            transformTagInfo(
                TagInfo.NoChildren(tagName, [], range)
                |> Tracer.create
                |> _.trace("Tag.NoChildren(tagName, range)")
            )
        | CallTagNoChildrenWithHandler tagCall ->
            transformTagInfo(tagCall |> Tracer.create |> _.trace("CallTagNoChildrenWithHandler tagCall"))
        | LetTagNoChildrenNoProps tagCall ->
            transformTagInfo(tagCall |> Tracer.create |> _.trace("LetTagNoChildrenNoProps tagCall"))
        | Tag.WithChildren callInfo ->
            transformTagInfo(
                TagInfo.WithChildren callInfo
                |> Tracer.create
                |> _.trace("Tag.WithChildren callInfo")
            )
        | Let.TagWithChildren tagCall ->
            transformTagInfo(tagCall |> Tracer.create |> _.trace("Let.TagWithChildren tagCall"))
        | Call.ImportTag(imp, range) ->
            transformTagInfo(
                TagInfo.NoChildren(LibraryImport imp, [], range)
                |> Tracer.create
                |> _.trace("Call.ImportTag(imp,range)")
            )
        | Let(_, Call.ImportTag(imp, range), Tag.WithChildren(_, callInfo, _)) ->
            transformTagInfo(
                TagInfo.WithChildren(LibraryImport imp, callInfo, range)
                |> Tracer.create
                |> _.trace("Let(_, Call.ImportTag(imp, range), Tag.WithChildren(_, callInfo, _))")
            )
        | Let(_, Call.ImportTag(imp, range), Sequential exprs) ->
            transformTagInfo(
                TagInfo.NoChildren(LibraryImport imp, exprs, range)
                |> Tracer.create
                |> _.trace("Let(_, Call.ImportTag(imp, range), Sequential exprs)")
            )
        | Let(_, Let(_, Call.ImportTag(imp, range), Sequential exprs), Tag.WithChildren(_, callInfo, _)) ->
            transformTagInfo(
                TagInfo.Combined(LibraryImport imp, exprs, callInfo, range)
                |> Tracer.create
                |> _.trace(
                    "Let(_, Let(_, Call.ImportTag(imp, range), Sequential exprs), Tag.WithChildren(_, callInfo, _))"
                )
            )
        | Let(name, value, expr) ->
            Tracer.ping("Let(name, value, expr)")
            Let(name, value, (transform expr))
        | Sequential expressions ->
            Tracer.ping("Sequential expressions")
            // transform only the last expression
            Sequential(
                expressions
                |> List.mapi(fun i expr -> if i = expressions.Length - 1 then transform expr else expr)
            )
        // transform children passed to component function call
        | Call(callee, callInfo, typ, range) ->
            Tracer.ping "Call(callee, callInfo, typ, range)"
            let newCallInfo = {
                callInfo with
                    Args = callInfo.Args |> List.map transform
            }
            Call(callee, newCallInfo, typ, range)
        | TextNoSiblings body ->
            Tracer.ping "TextNoSiblings body"
            body
        | IfThenElse(guardExpr, thenExpr, elseExpr, range) ->
            Tracer.ping "IfThenElse(guardExpr, thenExpr, elseExpr, range)"
            IfThenElse(guardExpr, transform thenExpr, transform elseExpr, range)
        | DecisionTree(decisionTree, targets) ->
            Tracer.ping "DecisionTree(decisionTree, targets)"
            DecisionTree(decisionTree, targets |> List.map(fun (target, expr) -> target, transform expr))
        | TypeCast(expr, DeclaredType _) ->
            Tracer.ping "TypeCast(expr, DeclaredType _)"
            transform expr
        | _ ->
            Tracer.ping "_"
            expr
    let transform (expr: Expr) = expr |> Tracer.create |> transform'
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

/// <summary>
/// Registers a function as a SolidComponent for transformation by
/// the <c>Oxpecker.Solid.FablePlugin</c>
/// </summary>
type SolidComponentAttribute() =
    inherit MemberDeclarationPluginAttribute()
    override _.FableMinimumVersion = "4.0"

    override this.Transform(pluginHelper: PluginHelper, file: File, memberDecl: MemberDecl) =
        PluginConfiguration.configure pluginHelper
        use fs = new IO.FileStream("PluginOutput.txt", IO.FileMode.Create)
        use sw = new IO.StreamWriter(fs)
        Console.SetOut sw
        if PluginConfiguration.Verbose() then
            enableTrace()
        // Console.WriteLine("!Start! MemberDecl")
        // Console.WriteLine(memberDecl.Body)
        // Console.WriteLine("!End! MemberDecl")
        let newBody =
            match memberDecl.Body with
            | Extended(Throw _, range) -> AST.transformException pluginHelper range
            | _ -> AST.transform memberDecl.Body
        let standardOutput = new IO.StreamWriter(Console.OpenStandardOutput())
        standardOutput.AutoFlush <- true
        Console.SetOut standardOutput
        { memberDecl with Body = newBody }

    override _.TransformCall(_: PluginHelper, _: MemberFunctionOrValue, expr: Expr) : Expr = expr
