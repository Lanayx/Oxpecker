module Rocket.FableTests

[<EntryPoint>]
let main _ =
    let cases = Rocket.RenderingCases.cases()
    let failures =
        cases
        |> List.choose(fun (name, actual, expected) ->
            if actual = expected then
                None
            else
                Some $"{name}: expected {expected}, got {actual}")
    if not failures.IsEmpty then
        failwith(String.concat "\n" failures)
    printfn "Passed %i shared rendering cases in JavaScript." cases.Length
    0
