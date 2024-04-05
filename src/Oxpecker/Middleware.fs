[<AutoOpen>]
module Oxpecker.Middleware

open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Logging

[<Extension>]
type ApplicationBuilderExtensions() =

    /// <summary>
    /// Uses ASP.NET Core's Endpoint Routing middleware to register Oxpecker endpoints.
    /// </summary>
    [<Extension>]
    static member UseOxpecker(builder: IApplicationBuilder, endpoints: Endpoint seq) =
        builder.UseEndpoints(fun builder -> builder.MapOxpeckerEndpoints endpoints)

[<Extension>]
type ServiceCollectionExtensions() =
    /// <summary>
    /// Adds default Oxpecker services to the ASP.NET Core service container.
    ///
    /// The default services include features like <see cref="Json.ISerializer"/>.
    /// </summary>
    /// <returns>Returns an <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> builder object.</returns>
    [<Extension>]
    static member AddOxpecker(svc: IServiceCollection) =
        svc.TryAddSingleton<Serializers.IJsonSerializer>(SystemTextJson.Serializer())
        svc.TryAddSingleton<ILogger>(fun sp ->
            let loggerFactory = sp.GetRequiredService<ILoggerFactory>()
            let webApp = sp.GetRequiredService<IWebHostEnvironment>()
            // see https://github.com/dotnet/aspnetcore/blob/main/src/DefaultBuilder/src/WebApplication.cs
            let categoryName = if isNull webApp then nameof WebApplication else webApp.ApplicationName
            loggerFactory.CreateLogger categoryName)
        svc
