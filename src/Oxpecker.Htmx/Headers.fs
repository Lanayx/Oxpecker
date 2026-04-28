namespace Oxpecker.Htmx

open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module HxRequestHeader =

    [<Literal>]
    let Request = "HX-Request"
    [<Literal>]
    let RequestType = "HX-Request-Type"
    [<Literal>]
    let CurrentUrl = "HX-Current-URL"
    [<Literal>]
    let Source = "HX-Source"
    [<Literal>]
    let Target = "HX-Target"
    [<Literal>]
    let Boosted = "HX-Boosted"
    [<Literal>]
    let HistoryRestoreRequest = "HX-History-Restore-Request"
    [<Literal>]
    let Accept = "Accept"
    [<Literal>]
    let LastEventID = "Last-Event-ID"

[<RequireQualifiedAccess>]
module HxResponseHeader =

    [<Literal>]
    let Trigger = "HX-Trigger"
    [<Literal>]
    let Location = "HX-Location"
    [<Literal>]
    let Redirect = "HX-Redirect"
    [<Literal>]
    let Refresh = "HX-Refresh"
    [<Literal>]
    let PushUrl = "HX-Push-Url"
    [<Literal>]
    let ReplaceUrl = "HX-Replace-Url"
    [<Literal>]
    let Reswap = "HX-Reswap"
    [<Literal>]
    let Retarget = "HX-Retarget"
    [<Literal>]
    let Reselect = "HX-Reselect"
