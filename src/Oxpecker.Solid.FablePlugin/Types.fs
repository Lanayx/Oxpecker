namespace Oxpecker.Solid.FablePlugin

open System
open Fable.AST
open Fable.AST.Fable


module Types =
    type Tracer<'T> = {
        Value: 'T
        Guid: Guid
        ConsoleColor: ConsoleColor
    }
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

    type PluginConfiguration() =
        static let mutable verbose = false
        static let mutable depth = 4
        static let mutable jsonify = true
        static let mutable slim = false
        static member configure(helper: Fable.PluginHelper) =
            if helper.Options.Define |> List.exists(fun s -> s.StartsWith "OXPECKER_SOLID_") then
                verbose <- true
                if
                    helper.Options.Define |> List.exists((=) "OXPECKER_SOLID_MINIMAL")
                    && not (helper.Options.Define |> List.exists(fun s -> s.StartsWith("OXPECKER_SOLID_DEBUG") || s = "OXPECKER_SOLID_TRACE"))
                then
                    jsonify <- false
                elif helper.Options.Define |> List.exists((=) "OXPECKER_SOLID_TRACE") then
                    depth <- 0
                else
                    for definition in
                        helper.Options.Define
                        |> List.filter(fun s -> s.StartsWith "OXPECKER_SOLID_DEBUG_") do
                        definition.ToCharArray()
                        |> Array.filter Char.IsDigit
                        |> String.Concat
                        |> int
                        |> fun i -> depth <- i
                if jsonify && helper.Options.Define |> List.exists((=) "OXPECKER_SOLID_MINIMAL")
                then slim <- true
        static member Verbose() = verbose
        static member Verbose value = verbose <- value
        static member Slim() = slim
        static member Depth() = depth
        static member Depth value = depth <- value
        static member Jsonify() = jsonify
        static member Jsonify value = jsonify <- value
