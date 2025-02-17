namespace rec Fable.Plugin.Tracer

open System
open System.Runtime.InteropServices
open System.Runtime.CompilerServices
open System.Text.Json // included post .net7 or 8
open Fable.AST
open Fable.AST.Fable
open Oxpecker.Solid.FablePlugin

[<AutoOpen>]
module TracerConfiguration =
    [<Literal>]
    let private minimal = "MINIMAL"
    [<Literal>]
    let private debug = "DEBUG"
    [<Literal>]
    let private trace = "TRACE"
    [<Literal>]
    let private file = "FILE"
    /// Extracts the num arg appended to the debug definition, if it is parseable, in an option.
    let private debugDepth (input: string) : int option =
        if input.StartsWith("DEBUG_") then Some input else None
        |> Option.map _.ToCharArray()
        |> Option.map(Array.filter Char.IsDigit)
        |> Option.map String.Concat
        |> Option.map int
    /// Convenience active recognizer that matches against an input list of strings, and outputs the first item, that matches the given value/string
    let (|Contains|_|) (value: string) (input: string list) = input |> List.tryFind((=) value)
    /// Convenience active recognizer that matches against an input list of strings, and outputs the first item, that starts with the given value/string
    let (|ContainsSomethingStartingWith|_|) (value: string) (input: string list) =
        input |> List.tryFind(_.StartsWith(value))
    /// <summary>
    /// Setting helper for the Fable Plugin Tracer.
    /// </summary>
    /// <example>
    /// To configure and enable the tracer, you need to provide a `Fable.PluginHelper` object
    /// (which is found in one of the transform methods) in the following way:
    /// <code>
    /// Settings.configure pluginHelper
    /// </code>
    /// <br/>
    /// To set the prefix for the fable defines that would active the tracer:
    /// <code>
    /// Settings.pluginName "OXPECKER_SOLID"
    /// </code>
    /// Which translates to usage such as
    /// <code>
    /// fable --define OXPECKER_SOLID_DEBUG
    /// </code>
    /// </example>
    [<RequireQualifiedAccess>]
    type Settings() =
        static let mutable PluginName = "PLUGIN_TRACER"
        static let mutable Verbose = false
        static let mutable Depth = 4
        static let mutable Jsonify = true
        static let mutable ToFile = false
        static let mutable FileName = ""
        static let mutable FileStream = null
        static let mutable StreamWriter = null
        static let StandardOutput =
            let this = new IO.StreamWriter(Console.OpenStandardOutput()) in
            this.AutoFlush <- true
            this
        static let mutable options = JsonSerializerOptions(JsonSerializerDefaults.General)
        static member configure (pluginName: string) (helper: Fable.PluginHelper) =
            PluginName <- pluginName
            if
                helper.Options.Define
                |> List.exists(fun definition -> definition.StartsWith $"{PluginName}_")
            then
                Verbose <- true
                let definitions =
                    helper.Options.Define
                    |> List.filter _.StartsWith($"{PluginName}_")
                    |> List.map _.Substring($"{PluginName}_" |> _.Length)
                let rec enable input =
                    match input with
                    | Contains file _ ->
                        ToFile <- true
                        FileName <- $"{PluginName}_{DateTime.Now.ToFileTime()}.txt"
                        FileStream <- new IO.FileStream(FileName, IO.FileMode.Append)
                        StreamWriter <- new IO.StreamWriter(FileStream)
                        StreamWriter.AutoFlush <- true
                        input |> List.filter((<>) file) |> enable
                    | Contains minimal _ & ContainsSomethingStartingWith debug recurse
                    | Contains minimal _ & Contains trace recurse ->
                        // TODO - the minimal flag combined with debug/trace is supposed to slim down the JSON output
                        Jsonify <- false
                        enable [ recurse ]
                    | Contains minimal _ -> Jsonify <- false
                    | ContainsSomethingStartingWith debug debugDefine ->
                        match debugDepth debugDefine with
                        | Some i -> Depth <- i
                        | None -> ()
                    | Contains trace _ -> Depth <- 0
                    | _ -> ()
                enable definitions
        static member verbose() = Verbose
        static member verbose value = Verbose <- value
        static member depth() = Depth
        static member depth value = Depth <- value
        static member jsonify() = Jsonify
        static member jsonify value = Jsonify <- value
        static member toFile() = ToFile
        static member toFile value = ToFile <- value
        static member canPipe() =
            if ToFile then
                Console.SetOut StreamWriter
        static member closePipe() =
            if ToFile then
                Console.SetOut StandardOutput
        static member converterOptions() = options
        static member addConverter converter = options.Converters.Add converter
        static member registerConverter<'T>() =
            JsonConverters.UnionConverter<'T>() |> Settings.addConverter
type Tracer =
    static member ping
        (
            [<Optional; DefaultParameterValue("")>] message: string,
            [<CallerMemberName; Optional; DefaultParameterValue("")>] memberName: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =
        let print () =
            Console.Write "                                     "
            Console.Write $"{memberName, -20}"
            Console.ResetColor()
            Console.WriteLine $"{message} ({path}:{line})"

        if Settings.verbose() then
            print()
            if Settings.toFile() then
                Settings.canPipe()
                print()
                Settings.closePipe()

type Tracer<'T> = {
    Value: 'T
    Guid: Guid
    ConsoleColor: ConsoleColor
} with
    member private this.emit
        (
            message: string,
            memberName: string,
            path: string,
            line: int,
            [<Optional; DefaultParameterValue(true)>] emitJson: bool
        ) =
        let print () =
            Console.ForegroundColor <- this.ConsoleColor
            Console.Write $"{this.Guid} "
            Console.ForegroundColor <- ConsoleColor.Gray
            Console.Write $"{memberName, -20}"
            Console.ResetColor()
            Console.WriteLine $"{message} ({path}:{line})"
        if Settings.verbose() then
            print()
            if Settings.jsonify() && emitJson then
                if Settings.toFile() then
                    Settings.canPipe()
                    print()
                    Console.WriteLine $"{PrettyPrinter.print this.Value}"
                    Settings.closePipe()
                else
                    Console.WriteLine $"{PrettyPrinter.print this.Value}"
    member this.trace
        (
            [<Optional; DefaultParameterValue("")>] message: string,
            [<CallerMemberName; Optional; DefaultParameterValue("")>] memberName: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =
        this.emit(message, memberName, path, line)
        this
    member this.ping
        (
            [<Optional; DefaultParameterValue("")>] message: string,
            [<CallerMemberName; Optional; DefaultParameterValue("")>] memberName: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =
        this.emit(message, memberName, path, line)
module Tracer =
    let private random = Random()

    let inline private _create value guid consoleColor = {
        Value = value
        Guid = guid
        ConsoleColor = consoleColor
    }

    let empty = _create () Guid.Empty ConsoleColor.Black
    let create value =
        if Settings.verbose() then
            random.Next(15) |> enum<ConsoleColor> |> _create value (Guid.NewGuid())
        else
            _create value Guid.Empty ConsoleColor.Black

    let bind (tracer: Tracer<'T>) (input: 'M) : Tracer<'M> =
        _create input tracer.Guid tracer.ConsoleColor

    let map (func: 'T -> 'M) (tracer: Tracer<'T>) = func tracer.Value |> bind tracer


module JsonConverters =
    open FSharp.Reflection

    let private incl converter = Settings.addConverter converter

    let (|IsUnion|_|) (input: 'T) =
        let typ = typeof<'T> in
        if FSharpType.IsUnion typ then
            FSharpValue.GetUnionFields(input, typ) |> Some
        else
            None

    [<AbstractClass>]
    type BasicConverter<'T>() =
        inherit Serialization.JsonConverter<'T>()
        override this.CanConvert typ = typ = typeof<'T>
        override this.Read(_, _, _) = unbox null

    type UnionConverter<'T>() =
        inherit BasicConverter<'T>()
        let typ = typeof<'T>
        override this.Write(writer, value, options) =
            match value with
            // In the case where the value is a union, we perform reflection on the union
            | IsUnion(unionValue, unionFields) ->
                let unionName = $"{typ.Name}.{unionValue.Name}"
                let fieldInfoArray = unionValue.GetFields()
                let (|NoFields|SingleField|MultipleFields|) (input: Reflection.PropertyInfo array) =
                    match input with
                    | [||] -> NoFields
                    | [| field |] -> SingleField field
                    | fields -> MultipleFields(fields |> Array.toList)
                match fieldInfoArray with
                | NoFields -> writer.WriteRawValue(sprintf "\"%s\"" unionName, true)
                // Union name is value
                | SingleField field ->
                    writer.WriteStartObject()
                    writer.WritePropertyName unionName
                    JsonSerializer.Serialize(writer, unionFields[0], field.PropertyType, options)
                    writer.WriteEndObject()
                // pass field directly to serializer
                | MultipleFields fields ->
                    // Embed the union name at the beginning of the object, and then follow up with the fields
                    writer.WriteStartObject()
                    writer.WriteString(unionName, "_")
                    // now we go through the fields and write the name of the field and then its value serialized
                    for (info, field) in List.zip fields (unionFields |> Array.toList) do
                        writer.WritePropertyName info.Name
                        JsonSerializer.Serialize(writer, field, info.PropertyType, options)
                    writer.WriteEndObject()
            // Every other type can be serialized natively, so we will pass them to the serializer to manage
            | _ -> JsonSerializer.Serialize(writer, value, typ, options)

    let register<'T> = UnionConverter<'T>() |> incl
    register<EntityPath>
    register<MemberRef>
    register<GeneratedMember>
    register<NumberInfo>
    register<NumberValue>
    register<ArrayKind>
    register<Constraint>
    register<Fable.Type>
    register<Fable.Expr>
    register<ValueKind>
    register<ImportKind>
    register<Declaration>
    register<NewArrayKind>
    register<GetKind>
    register<SetKind>
    register<TestKind>
    register<ExtendedSet>
    register<UnresolvedExpr>
    register<OperationKind>
    register<NumberKind>
    register<RegexFlag>
    register<UnaryOperator>
    register<BinaryOperator>
    register<LogicalOperator>
    register<TagInfo>
    register<TagSource>
type PrettyPrinter =
    static member print(input: 'T) : string =
        JsonSerializer.Serialize(input, Settings.converterOptions())
