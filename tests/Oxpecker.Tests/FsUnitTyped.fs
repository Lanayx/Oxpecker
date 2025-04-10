namespace FsUnitTyped

[<AutoOpen>]
module CustomTopLevelOperators =

    open FsUnit.Xunit
    open System.Diagnostics
    open NHamcrest

    [<DebuggerStepThrough>]
    let shouldFailWithMessage<'exn when 'exn :> exn> message (f: unit -> unit) =
        f |> should (throwWithMessage message) typeof<'exn>

    [<DebuggerStepThrough>]
    let shouldEqualSeq (expected: #seq<'a>) (actual: #seq<'a>) =
        actual |> should equalSeq expected

    let private structuallyEqual expected = Is.StructurallyEqualTo(expected)

    [<DebuggerStepThrough>]
    let shouldStructuallyEqual<'a> (expected: 'a) (actual: 'a) =
        actual |> should structuallyEqual expected
