namespace Oxpecker.Htmx

open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module HxRequestHeader =

    [<Literal>]
    let Boosted = "HX-Boosted"
    [<Literal>]
    let CurrentUrl = "HX-Current-URL"
    [<Literal>]
    let HistoryRestoreRequest = "HX-History-Restore-Request"
    [<Literal>]
    let Prompt = "HX-Prompt"
    [<Literal>]
    let Request = "HX-Request"
    [<Literal>]
    let Target = "HX-Target"
    [<Literal>]
    let TriggerName = "HX-Trigger-Name"
    [<Literal>]
    let Trigger = "HX-Trigger"

[<RequireQualifiedAccess>]
module HxResponseHeader =

    [<Literal>]
    let Location = "HX-Location"
    [<Literal>]
    let PushUrl = "HX-Push-Url"
    [<Literal>]
    let Redirect = "HX-Redirect"
    [<Literal>]
    let Refresh = "HX-Refresh"
    [<Literal>]
    let ReplaceUrl = "HX-Replace-Url"
    [<Literal>]
    let Reswap = "HX-Reswap"
    [<Literal>]
    let Retarget = "HX-Retarget"
    [<Literal>]
    let Trigger = "HX-Trigger"
    [<Literal>]
    let TriggerAfterSettle = "HX-Trigger-After-Settle"
    [<Literal>]
    let TriggerAfterSwap = "HX-Trigger-After-Swap"
