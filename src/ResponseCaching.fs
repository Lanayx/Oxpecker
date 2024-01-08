namespace Oxpecker

open System.Threading.Tasks

[<AutoOpen>]
module ResponseCaching =
    open System
    open Microsoft.Net.Http.Headers
    open Microsoft.AspNetCore.Http
    open Microsoft.AspNetCore.ResponseCaching

    /// <summary>
    /// Specifies the directive for the `Cache-Control` HTTP header:
    ///
    /// NoCache: The resource should not be cached under any circumstances.
    /// Cache: Cache with options
    /// </summary>
    type CacheDirective =
        | NoCache
        | Cache of CacheControlHeaderValue

    let private noCacheHeader = CacheControlHeaderValue(NoCache = true, NoStore = true)

    let inline private cacheHeader isPublic duration =
        CacheControlHeaderValue(
            Public = isPublic,
            MaxAge = Nullable duration)

    /// <summary>
    /// Enables (or suppresses) response caching by clients and proxy servers.
    /// This http handler integrates with ASP.NET Core's response caching middleware.
    ///
    /// The responseCaching http handler will set the relevant HTTP response headers in order to enable response caching on the client, by proxies (if public) and by the ASP.NET Core middleware (if enabled).
    /// </summary>
    /// <param name="directive">Specifies the cache directive to be set in the response's HTTP headers. Use NoCache to suppress caching altogether or use Cache to enable caching.</param>
    /// <param name="vary">Optionally specify which HTTP headers have to match in order to return a cached response (e.g. Accept and/or Accept-Encoding).</param>
    /// <param name="varyByQueryKeys">An optional list of query keys which will be used by the ASP.NET Core response caching middleware to vary (potentially) cached responses. If this parameter is used then the ASP.NET Core response caching middleware has to be enabled. For more information check the official [VaryByQueryKeys](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-2.1#varybyquerykeys) documentation.</param>
    /// <param name="ctx"></param>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let responseCaching (directive: CacheDirective)
                        (vary: string option)
                        (varyByQueryKeys: string[] option): EndpointHandler =
        fun (ctx: HttpContext) ->

            let tHeaders = ctx.Response.GetTypedHeaders()
            let headers  = ctx.Response.Headers

            match directive with
            | NoCache ->
                tHeaders.CacheControl          <- noCacheHeader
                headers[ HeaderNames.Pragma ]  <- "no-cache"
                headers[ HeaderNames.Expires ] <- "-1"
            | Cache control  ->
                tHeaders.CacheControl <- control

            if vary.IsSome then
                headers[HeaderNames.Vary] <- vary.Value

            if varyByQueryKeys.IsSome then
                let responseCachingFeature = ctx.Features.Get<IResponseCachingFeature>()
                if isNotNull responseCachingFeature then
                    responseCachingFeature.VaryByQueryKeys <- varyByQueryKeys.Value

            Task.CompletedTask

    /// <summary>
    /// Disables response caching by clients and proxy servers.
    /// </summary>
    /// <returns>An Oxpecker `HttpHandler` function which can be composed into a bigger web application.</returns>
    let noResponseCaching: EndpointHandler = responseCaching NoCache None None