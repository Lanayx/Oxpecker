namespace JetBrains.Annotations

open System
open Fable.Core

[<Erase>]
type internal InjectedLanguage =
    | CSS = 0
    | HTML = 1
    | JAVASCRIPT = 2
    | JSON = 3
    | XML = 4

[<AttributeUsage(AttributeTargets.Parameter
                 ||| AttributeTargets.Field
                 ||| AttributeTargets.Property)>]
[<Erase>]
type internal LanguageInjectionAttribute(injectedLanguage: InjectedLanguage) =
    inherit Attribute()
    [<Erase>]
    member x.InjectedLanguage = injectedLanguage
    [<Erase>]
    member val Prefix = "" with get, set
    [<Erase>]
    member val Suffix = "" with get, set
