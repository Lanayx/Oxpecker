namespace Oxpecker.Htmx

open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module HxRequestHeader =

    /// `HX-Request` — always `"true"`; indicates the request was issued by htmx.
    [<Literal>]
    let Request = "HX-Request"
    /// `HX-Request-Type` — the kind of request: `"full"` (whole document) or `"partial"` (fragment). New in htmx 4.
    [<Literal>]
    let RequestType = "HX-Request-Type"
    /// `HX-Current-URL` — the current URL shown in the browser's location bar.
    [<Literal>]
    let CurrentUrl = "HX-Current-URL"
    /// `HX-Source` — identifier (`tag#id` format) of the element that triggered the request. Replaces the old `HX-Trigger` request header. New in htmx 4.
    [<Literal>]
    let Source = "HX-Source"
    /// `HX-Target` — the `id` of the target element, if it has one. Format: <tag>#<id> (e.g. div#results) or just <tag> for elements without an ID. This header is only included when a target is specified.
    [<Literal>]
    let Target = "HX-Target"
    /// `HX-Boosted` — present when the request was made by a `hx-boost`-ed element.
    [<Literal>]
    let Boosted = "HX-Boosted"
    /// `HX-History-Restore-Request` — `"true"` when htmx is fetching content to restore a history entry after a cache miss.
    [<Literal>]
    let HistoryRestoreRequest = "HX-History-Restore-Request"
    /// `Accept` — the content types htmx will accept from the server.
    [<Literal>]
    let Accept = "Accept"
    /// `Last-Event-ID` — the last received SSE event ID, sent on reconnection for stream resumption.
    [<Literal>]
    let LastEventId = "Last-Event-ID"
    /// `HX-Preloaded` — sent on requests issued by the `hx-preload` extension.
    [<Literal>]
    let Preloaded = "HX-Preloaded"
    /// `HX-PTag` — polling tag sent back to the server by the `hx-ptag` extension.
    [<Literal>]
    let PTag = "HX-PTag"
    /// `HX-Prompt` — the user's answer to the `hx-prompt` dialog, sent URI-encoded by the `hx-prompt` extension (decode it with `Uri.UnescapeDataString`).
    [<Literal>]
    let Prompt = "HX-Prompt"
    /// `HX-Last-Part-ID` — the `HX-Part-ID` of the last completed part, sent by the `hx-multipart` extension when reconnecting so the server can resume the stream.
    [<Literal>]
    let LastPartId = "HX-Last-Part-ID"

[<RequireQualifiedAccess>]
module HxResponseHeader =

    /// `HX-Trigger` — trigger client-side events after the response is processed.
    [<Literal>]
    let Trigger = "HX-Trigger"
    /// `HX-Location` — perform a client-side redirect that issues a new htmx request instead of a full page reload.
    [<Literal>]
    let Location = "HX-Location"
    /// `HX-Redirect` — perform a client-side redirect to a new location with a full page reload.
    [<Literal>]
    let Redirect = "HX-Redirect"
    /// `HX-Refresh` — when set to `"true"`, the client does a full refresh of the page.
    [<Literal>]
    let Refresh = "HX-Refresh"
    /// `HX-Push-Url` — push a new URL into the browser history stack.
    [<Literal>]
    let PushUrl = "HX-Push-Url"
    /// `HX-Replace-Url` — replace the current URL in the browser's location bar.
    [<Literal>]
    let ReplaceUrl = "HX-Replace-Url"
    /// `HX-Reswap` — override how the response is swapped in (an `hx-swap` value).
    [<Literal>]
    let Reswap = "HX-Reswap"
    /// `HX-Retarget` — a CSS selector overriding the element the response is swapped into.
    [<Literal>]
    let Retarget = "HX-Retarget"
    /// `HX-Reselect` — a CSS selector choosing which part of the response is swapped in (an `hx-select` value).
    [<Literal>]
    let Reselect = "HX-Reselect"
    /// `HX-Download` — set by the server so the `hx-download` extension fetches the given URL as a file download.
    [<Literal>]
    let Download = "HX-Download"
    /// `HX-PTag` — polling tag stored by the `hx-ptag` extension and echoed on the next request.
    [<Literal>]
    let PTag = "HX-PTag"
    /// `HX-Part-ID` — per-part identifier set on a `multipart/mixed` part by the server, remembered by the `hx-multipart` extension and sent back as `HX-Last-Part-ID` on reconnect.
    [<Literal>]
    let PartId = "HX-Part-ID"
