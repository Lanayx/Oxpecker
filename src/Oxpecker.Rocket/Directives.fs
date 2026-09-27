namespace Oxpecker.Rocket

open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine

[<RequireQualifiedAccess>]
module RocketModifier =
    /// Keeps a keyed signal-name attribute in the outer page scope.
    [<Literal>]
    let root = "__root"

/// Structural directives understood by Rocket on template elements only.
type RocketTemplateExtensions =
    /// Mounts the template content while the expression is true.
    [<Extension>]
    static member dataIf(this: template, [<StringSyntax("js")>] value: string | null) = this.attr("data-if", value)
    /// Mounts this branch when earlier branches were not taken and the expression is true.
    [<Extension>]
    static member dataElseIf(this: template, [<StringSyntax("js")>] value: string | null) =
        this.attr("data-else-if", value)
    /// Mounts this branch when no earlier branch was taken.
    [<Extension>]
    static member dataElse(this: template) = this.bool("data-else", true)
    /// Repeats the template for an iterable, e.g. item, index in $$items.
    [<Extension>]
    static member dataFor(this: template, [<StringSyntax("js")>] value: string | null) = this.attr("data-for", value)
