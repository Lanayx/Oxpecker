namespace FsUnitTyped

[<AutoOpen>]
module CustomTopLevelOperators =

    open System.Diagnostics
    open FsUnit.Xunit
    open Xunit

    [<DebuggerStepThrough>]
    let shouldFailWithMessage<'exn when 'exn :> exn> message (f: unit -> unit) =
        f |> should (throwWithMessage message) typeof<'exn>

    [<DebuggerStepThrough>]
    let shouldEqualSeq (expected: #seq<'a>) (actual: #seq<'a>) = actual |> should equalSeq expected

    [<DebuggerStepThrough>]
    let shouldEquivalent<'a> (expected: 'a) (actual: 'a) = Assert.Equivalent(expected, actual)
