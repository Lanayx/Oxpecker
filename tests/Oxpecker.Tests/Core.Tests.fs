module Oxpecker.Tests.Core

open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Features
open Oxpecker
open Xunit
open FsUnitTyped

type StartedHttpResponse() =
    inherit HttpResponseFeature()
    override this.get_HasStarted() = true

[<Fact>]
let ``Compose two handlers, both executed``() =
    task {
        let ctx = DefaultHttpContext()
        let mutable x = 0
        let handler1: EndpointHandler = fun _ -> task { x <- x + 1 }
        let handler2: EndpointHandler = fun _ -> task { x <- x + 2 }

        do! (handler1 >=> handler2) ctx

        x |> shouldEqual 3
    }

[<Fact>]
let ``Compose two handlers, none executed``() =
    task {
        let ctx = DefaultHttpContext()
        ctx.Features.Set<IHttpResponseFeature>(StartedHttpResponse())
        ctx.Response.Body <- new MemoryStream()
        let mutable x = 0
        let handler1: EndpointHandler = fun _ -> task { x <- x + 1; }
        let handler2: EndpointHandler = fun _ -> task { x <- x + 2 }

        do! (handler1 >=> handler2) ctx

        x |> shouldEqual 0
    }

[<Fact>]
let ``Compose two handlers, only first executed``() =
    task {
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        let mutable x = 0
        let handler1: EndpointHandler = fun _ -> task { x <- x + 1; ctx.Features.Set<IHttpResponseFeature>(StartedHttpResponse()) }
        let handler2: EndpointHandler = fun _ -> task { x <- x + 2 }

        do! (handler1 >=> handler2) ctx

        x |> shouldEqual 1
    }

[<Fact>]
let ``Compose middleware and handler, both executed``() =
    task {
        let ctx = DefaultHttpContext()
        let mutable x = 0
        let middleware: EndpointMiddleware = fun next c -> task { x <- x + 1; do! next c }
        let handler: EndpointHandler = fun _ -> task { x <- x + 2 }

        do! (middleware >=> handler) ctx

        x |> shouldEqual 3
    }

[<Fact>]
let ``Compose middleware and handler, none executed``() =
    task {
        let ctx = DefaultHttpContext()
        ctx.Features.Set<IHttpResponseFeature>(StartedHttpResponse())
        ctx.Response.Body <- new MemoryStream()
        let mutable x = 0
        let middleware: EndpointMiddleware = fun next c -> task { x <- x + 1; do! next c }
        let handler: EndpointHandler = fun _ -> task { x <- x + 2 }

        do! (middleware >=> handler) ctx

        x |> shouldEqual 0
    }

[<Fact>]
let ``Compose middleware and handler, only first executed``() =
    task {
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        let mutable x = 0
        let middleware: EndpointMiddleware = fun _ _ -> task { x <- x + 1; }
        let handler: EndpointHandler = fun _ -> task { x <- x + 2 }

        do! (middleware >=> handler) ctx
        x |> shouldEqual 1
    }

[<Fact>]
let ``Compose two middlewares, both executed``() =
    task {
        let ctx = DefaultHttpContext()
        let mutable x = 0
        let middlware1: EndpointMiddleware = fun next ctx -> task { x <- x + 1; return! next ctx }
        let middlware2: EndpointMiddleware = fun next ctx -> task { x <- x + 2; return! next ctx }
        let handler: EndpointHandler = fun _ -> Task.CompletedTask

        do! (middlware1 >=> middlware2 >=> handler) ctx

        x |> shouldEqual 3
    }

[<Fact>]
let ``Compose two middlewares, none executed``() =
    task {
        let ctx = DefaultHttpContext()
        ctx.Features.Set<IHttpResponseFeature>(StartedHttpResponse())
        ctx.Response.Body <- new MemoryStream()
        let mutable x = 0
        let middlware1: EndpointMiddleware = fun next ctx -> task { x <- x + 1; return! next ctx }
        let middlware2: EndpointMiddleware = fun next ctx -> task { x <- x + 2; return! next ctx }
        let handler: EndpointHandler = fun _ -> Task.CompletedTask

        do! (middlware1 >=> middlware2 >=> handler) ctx

        x |> shouldEqual 0
    }

[<Fact>]
let ``Compose two middlewares, only first executed``() =
    task {
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        let mutable x = 0
        let middlware1: EndpointMiddleware = fun next ctx -> task { x <- x + 1; }
        let middlware2: EndpointMiddleware = fun next ctx -> task { x <- x + 2; return! next ctx }
        let handler: EndpointHandler = fun _ -> Task.CompletedTask

        do! (middlware1 >=> middlware2 >=> handler) ctx

        x |> shouldEqual 1
    }