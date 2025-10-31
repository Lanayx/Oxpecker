[<AutoOpen>]
module Oxpecker.Middleware

open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Logging

type ApplicationBuilderExtensions() =

    /// <summary>
    /// Uses ASP.NET Core's Endpoint Routing middleware to register Oxpecker endpoints.
    /// </summary>
    [<Extension>]
    static member UseOxpecker(builder: IApplicationBuilder, endpoints: Endpoint seq) =
        let addAntiforgery =
            match builder.ApplicationServices.GetService(typeof<IAntiforgery>) with
            | null -> false
            | _ -> true
        builder.UseEndpoints(_.MapOxpeckerEndpoints(endpoints, addAntiforgery))

    /// <summary>
    /// Uses ASP.NET Core's Endpoint Routing middleware to register single Oxpecker endpoint.
    /// </summary>
    [<Extension>]
    static member UseOxpecker(builder: IApplicationBuilder, endpoint: Endpoint) =
        let addAntiforgery =
            match builder.ApplicationServices.GetService(typeof<IAntiforgery>) with
            | null -> false
            | _ -> true
        builder.UseEndpoints(_.MapOxpeckerEndpoint(endpoint, addAntiforgery))

type ServiceCollectionExtensions() =
    /// <summary>
    /// Adds default Oxpecker services to the ASP.NET Core service container.
    ///
    /// The default services include features like <see cref="Json.ISerializer"/>.
    /// </summary>
    /// <returns>Returns an <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> builder object.</returns>
    [<Extension>]
    static member AddOxpecker(svc: IServiceCollection) =
        svc.TryAddSingleton<IJsonSerializer>(SystemTextJsonSerializer())
        svc.TryAddSingleton<IModelBinder>(ModelBinder())
        svc.TryAddSingleton<ILogger>(fun sp ->
            let loggerFactory = sp.GetRequiredService<ILoggerFactory>()
            let webApp = sp.GetRequiredService<IWebHostEnvironment>()
            loggerFactory.CreateLogger webApp.ApplicationName)
        svc
