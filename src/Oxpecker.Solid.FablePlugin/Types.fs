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
