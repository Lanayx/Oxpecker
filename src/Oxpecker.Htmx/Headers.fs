namespace Oxpecker.Htmx

open Microsoft.FSharp.Core

module HxHeader =

    [<RequireQualifiedAccess>]
    module Request =
        let [<Literal>] Boosted = "HX-Boosted"
        let [<Literal>] CurrentUrl = "HX-Current-URL"
        let [<Literal>] HistoryRestoreRequest = "HX-History-Restore-Request"
        let [<Literal>] Prompt = "HX-Prompt"
        let [<Literal>] Request = "HX-Request"
        let [<Literal>] Target = "HX-Target"
        let [<Literal>] TriggerName = "HX-Trigger-Name"
        let [<Literal>] Trigger = "HX-Trigger"

    [<RequireQualifiedAccess>]
    module Response =
        let [<Literal>] Location = "HX-Location"
        let [<Literal>] PushUrl = "HX-Push-Url"
        let [<Literal>] Redirect = "HX-Redirect"
        let [<Literal>] Refresh = "HX-Refresh"
        let [<Literal>] ReplaceUrl = "HX-Replace-Url"
        let [<Literal>] Reswap = "HX-Reswap"
        let [<Literal>] Retarget = "HX-Retarget"
        let [<Literal>] Trigger = "HX-Trigger"
        let [<Literal>] TriggerAfterSettle = "HX-Trigger-After-Settle"
        let [<Literal>] TriggerAfterSwap = "HX-Trigger-After-Swap"
