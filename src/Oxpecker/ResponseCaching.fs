namespace Oxpecker

open System.Threading.Tasks

[<AutoOpen>]
module ResponseCaching =
    open System
    open Microsoft.Net.Http.Headers
    open Microsoft.AspNetCore.Http
    open Microsoft.AspNetCore.ResponseCaching

    let private noCacheHeader = CacheControlHeaderValue(NoCache = true, NoStore = true)

    let inline private cacheHeader isPublic duration =
        CacheControlHeaderValue(Public = isPublic, MaxAge = Nullable duration)

    /// <summary>
    /// Enables (or suppresses) response caching by clients and proxy servers.
    /// This http handler integrates with ASP.NET Core's response caching middleware.
    ///
    /// The responseCaching http handler will set the relevant HTTP response headers in order to enable response caching on the client, by proxies (if public) and by the ASP.NET Core middleware (if enabled).
    /// </summary>
    /// <param name="cacheControl">Specifies the cache control to be set in the response's HTTP headers. Use None to suppress caching altogether or use Some to enable caching.</param>
    /// <param name="vary">Optionally specify which HTTP headers have to match in order to return a cached response (e.g. Accept and/or Accept-Encoding).</param>
    /// <param name="varyByQueryKeys">An optional list of query keys which will be used by the ASP.NET Core response caching middleware to vary (potentially) cached responses. If this parameter is used then the ASP.NET Core response caching middleware has to be enabled. For more information check the official [VaryByQueryKeys](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-2.1#varybyquerykeys) documentation.</param>
    /// <param name="ctx"></param>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let responseCaching
        (cacheControl: CacheControlHeaderValue option)
        (vary: string option)
        (varyByQueryKeys: string[] option)
        : EndpointHandler =
        fun (ctx: HttpContext) ->

            let tHeaders = ctx.Response.GetTypedHeaders()
            let headers = ctx.Response.Headers

            match cacheControl with
            | None ->
                tHeaders.CacheControl <- noCacheHeader
                headers.Pragma <- "no-cache"
                headers.Expires <- "-1"
            | Some control -> tHeaders.CacheControl <- control

            if vary.IsSome then
                headers.Vary <- vary.Value

            if varyByQueryKeys.IsSome then
                let responseCachingFeature = ctx.Features.Get<IResponseCachingFeature>()
                if isNotNull responseCachingFeature then
                    responseCachingFeature.VaryByQueryKeys <- varyByQueryKeys.Value

            Task.CompletedTask

    /// <summary>
    /// Disables response caching by clients and proxy servers.
    /// </summary>
    /// <returns>An Oxpecker `HttpHandler` function which can be composed into a bigger web application.</returns>
    let noResponseCaching: EndpointHandler = responseCaching None None None
