namespace JetBrains.Annotations

open System

[<Struct>]
type internal InjectedLanguage =
    | CSS = 0
    | HTML = 1
    | JAVASCRIPT = 2
    | JSON = 3
    | XML = 4

[<AttributeUsage(AttributeTargets.Parameter
                 ||| AttributeTargets.Field
                 ||| AttributeTargets.Property)>]
type internal LanguageInjectionAttribute(injectedLanguage: InjectedLanguage) =
    inherit Attribute()
    member x.InjectedLanguage = injectedLanguage
    member val Prefix = "" with get, set
    member val Suffix = "" with get, set
