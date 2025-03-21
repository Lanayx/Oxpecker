module Oxpecker.Solid.Tests.SolidCases.Router

open Oxpecker.Solid
open Oxpecker.Solid.Router
open Oxpecker.Solid.Tests.SolidCases.App1
open Oxpecker.Solid.Tests.SolidCases.App2

[<SolidComponent>]
let Root (props: RootProps) : HtmlElement =
    Fragment() {
        h1() { "Root header" }
        props.children
        A(href="/about") { "About" }
    }

[<SolidComponent>]
let App3 () : HtmlElement =
    let navigator = useNavigate()
    createEffect(fun _ -> navigator.Invoke("/def/inner"))

    h1() { "Hello world 3" }

[<SolidComponent>]
let Test1 () =
    Router(root=Root) {
        Route()
        Route(path="/def") {
            Route(path="/inner", component'=App1)
        }
        Route(path="/ghi", component'=App2)
    }

[<SolidComponent>]
let Test2() =
    let routes = [|
        RootConfig("/app1", lazy'(fun () -> importComponent("/App1")))
        RootConfig("/app2", lazy'(fun () -> importComponent("/App2")))
    |]

    Router() {
        routes
    }
