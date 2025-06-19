namespace FsUnit.Light

[<AutoOpen>]
module CustomTopLevelOperators =

    open System.Diagnostics
    open FsUnit.Light.Xunit
    open Xunit

    [<DebuggerStepThrough>]
    let shouldFailWithMessage<'exn when 'exn :> exn> message (f: unit -> unit) =
        Assert.Throws<'exn> f |> _.Message |> shouldEqual message

    [<DebuggerStepThrough>]
    let shouldEquivalent<'a> (expected: 'a) (actual: 'a) = Assert.Equivalent(expected, actual)
