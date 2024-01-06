[<AutoOpen>]
module Oxpecker.Middleware

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
        (builder : IApplicationBuilder,
        endpoints: Endpoint seq) =

        builder.UseEndpoints(fun builder -> builder.MapOxpeckerEndpoints endpoints)