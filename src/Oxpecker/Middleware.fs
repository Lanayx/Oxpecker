[<AutoOpen>]
module Oxpecker.Middleware

open System.Runtime.CompilerServices
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
        builder.UseEndpoints(_.MapOxpeckerEndpoints(endpoints))

    /// <summary>
    /// Uses ASP.NET Core's Endpoint Routing middleware to register a single Oxpecker endpoint.
    /// </summary>
    [<Extension>]
    static member UseOxpecker(builder: IApplicationBuilder, endpoint: Endpoint) =
        builder.UseEndpoints(_.MapOxpeckerEndpoint(endpoint))

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
