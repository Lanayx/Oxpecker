module Headers.Tests

open Oxpecker.Htmx
open Xunit
open FsUnit.Light

// ─── Request headers ───

[<Fact>]
let ``HxRequestHeader.Request equals HX-Request`` () =
    HxRequestHeader.Request |> shouldEqual "HX-Request"

[<Fact>]
let ``HxRequestHeader.RequestType equals HX-Request-Type`` () =
    HxRequestHeader.RequestType |> shouldEqual "HX-Request-Type"

[<Fact>]
let ``HxRequestHeader.CurrentUrl equals HX-Current-URL`` () =
    HxRequestHeader.CurrentUrl |> shouldEqual "HX-Current-URL"

[<Fact>]
let ``HxRequestHeader.Source equals HX-Source`` () =
    HxRequestHeader.Source |> shouldEqual "HX-Source"

[<Fact>]
let ``HxRequestHeader.Target equals HX-Target`` () =
    HxRequestHeader.Target |> shouldEqual "HX-Target"

[<Fact>]
let ``HxRequestHeader.Boosted equals HX-Boosted`` () =
    HxRequestHeader.Boosted |> shouldEqual "HX-Boosted"

[<Fact>]
let ``HxRequestHeader.HistoryRestoreRequest equals HX-History-Restore-Request`` () =
    HxRequestHeader.HistoryRestoreRequest
    |> shouldEqual "HX-History-Restore-Request"

// ─── Response headers ───

[<Fact>]
let ``HxResponseHeader.Trigger equals HX-Trigger`` () =
    HxResponseHeader.Trigger |> shouldEqual "HX-Trigger"

[<Fact>]
let ``HxResponseHeader.Location equals HX-Location`` () =
    HxResponseHeader.Location |> shouldEqual "HX-Location"

[<Fact>]
let ``HxResponseHeader.Redirect equals HX-Redirect`` () =
    HxResponseHeader.Redirect |> shouldEqual "HX-Redirect"

[<Fact>]
let ``HxResponseHeader.Refresh equals HX-Refresh`` () =
    HxResponseHeader.Refresh |> shouldEqual "HX-Refresh"

[<Fact>]
let ``HxResponseHeader.PushUrl equals HX-Push-Url`` () =
    HxResponseHeader.PushUrl |> shouldEqual "HX-Push-Url"

[<Fact>]
let ``HxResponseHeader.ReplaceUrl equals HX-Replace-Url`` () =
    HxResponseHeader.ReplaceUrl |> shouldEqual "HX-Replace-Url"

[<Fact>]
let ``HxResponseHeader.Reswap equals HX-Reswap`` () =
    HxResponseHeader.Reswap |> shouldEqual "HX-Reswap"

[<Fact>]
let ``HxResponseHeader.Retarget equals HX-Retarget`` () =
    HxResponseHeader.Retarget |> shouldEqual "HX-Retarget"

[<Fact>]
let ``HxResponseHeader.Reselect equals HX-Reselect`` () =
    HxResponseHeader.Reselect |> shouldEqual "HX-Reselect"

// ─── Removed headers should not exist ───
// These are compile-time guarantees: HxRequestHeader.Trigger, HxRequestHeader.TriggerName,
// HxRequestHeader.Prompt, HxResponseHeader.TriggerAfterSettle, HxResponseHeader.TriggerAfterSwap
// are no longer defined. Any code referencing them will fail to compile.
