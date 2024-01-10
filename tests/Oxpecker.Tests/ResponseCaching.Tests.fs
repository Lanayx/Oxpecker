module Oxpecker.Tests.ResponseCaching

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.ResponseCaching
open Microsoft.Net.Http.Headers
open Oxpecker
open Xunit
open FsUnitTyped

#nowarn "3391"

[<Fact>]
let ``ResponseCaching should cache responses`` () =
    task {
        let ctx = DefaultHttpContext()

        do! noResponseCaching ctx

        ctx.Response.Headers.CacheControl |> shouldEqual "no-store, no-cache"
        ctx.Response.Headers.Pragma |> shouldEqual "no-cache"
        ctx.Response.Headers.Expires |> shouldEqual "-1"
    }

[<Fact>]
let ``ResponseCaching should cache responses with a max age and a shared max age`` () =
    task {
        let ctx = DefaultHttpContext()
        let controlHeader = CacheControlHeaderValue()
        controlHeader.Public <- true
        controlHeader.MaxAge <- TimeSpan.FromSeconds(10.)
        controlHeader.SharedMaxAge <- TimeSpan.FromSeconds(20.)

        do! responseCaching (Some controlHeader) None None ctx

        ctx.Response.Headers.CacheControl |> shouldEqual "public, max-age=10, s-maxage=20"
    }

[<Fact>]
let ``ResponseCaching should vary by header`` () =
    task {
        let ctx = DefaultHttpContext()
        let controlHeader = CacheControlHeaderValue()

        do! responseCaching (Some controlHeader) (Some "Accept") None ctx

        ctx.Response.Headers.Vary |> shouldEqual "Accept"
    }

[<Fact>]
let ``ResponseCaching should vary by query string`` () =
    task {
        let ctx = DefaultHttpContext()
        let feature = ResponseCachingFeature()
        ctx.Features.Set<IResponseCachingFeature>(feature)
        let controlHeader = CacheControlHeaderValue()

        do! responseCaching (Some controlHeader) None (Some [|"foo"|]) ctx

        feature.VaryByQueryKeys |> shouldEqual [|"foo"|]
    }
