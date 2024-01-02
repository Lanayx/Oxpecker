[<AutoOpen>]
module Oxpecker.Middleware

open System
open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Builder
open Oxpecker.Routing

[<Extension>]
type ApplicationBuilderExtensions() =

    /// <summary>
    /// Uses ASP.NET Core's Endpoint Routing middleware to register Oxpecker endpoints.
    /// </summary>
    [<Extension>]
    static member UseOxpecker
        (builder  : IApplicationBuilder,
        endpoints : Endpoint seq) =

        builder.UseEndpoints(fun builder -> builder.MapOxpeckerEndpoints endpoints)

    /// <summary>
    /// Uses ASP.NET Core's Endpoint Routing middleware to register Oxpecker endpoints.
    /// </summary>
    [<Extension>]
    static member UseOxpecker2
        (builder  : IApplicationBuilder,
        endpoints : Oxpecker.Routing2.Routers.Endpoint2 seq) =

        builder.UseEndpoints(fun builder -> Oxpecker.Routing2.EndpointRouteBuilderExtensions.MapOxpeckerEndpoints2(builder, endpoints))