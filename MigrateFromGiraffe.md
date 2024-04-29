# Migration From Giraffe

While Oxpecker is mostly oriented at developers building brand-new projects, some people might want to migrate their Giraffe applications to Oxpecker to get better support.

Oxpecker API is very similar to Giraffe, however there are some breaking changes you need to be aware of. There are three main sections for migrating from Giraffe Standard Routing, from Giraffe Endpoint Routing and common set of changes for both.

- [Common Changes](#common-changes)
- [Migrate from Endpoint Routing](#migrate-from-endpoint-routing)
- [Migrate from Standard Routing](#migrate-from-standard-routing)

## Common Changes

### HTTP Handler Type

```fsharp
// Giraffe
type HttpFunc = HttpContext -> Task<HttpContext option>
type HttpHandler = HttpFunc -> HttpContext -> Task<HttpContext option>

// Oxpecker
type EndpointHandler = HttpContext -> Task
type EndpointMiddleware = EndpointHandler -> HttpContext -> Task
```
In Giraffe you mostly work with `HttpHandler`, which means that every handler takes additional _next_ parameter. In Oxpecker most of the time you work with `EndpointHandler` and don't need _next_ (except when using `EndpointMiddleware`).

```fsharp
// Giraffe
let someHandler (str: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            return! ctx.WriteTextAsync str
        }

// Oxpecker
let someHandler (str: string) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            return! ctx.WriteText str
        }
```

### View Engine

Oxpecker ViewEngine was written from scratch using computation expressions instead of lists to give you better readability. But from migration perspective, it would be inefficient to rewrite all your views to the new engine. So it's recommended you add additional `HttpContext` extension method to use Giraffe views in Oxpecker and use it instead of Oxpecker's `WriteHtmlView` method.

```fsharp
open Oxpecker
open Giraffe.ViewEngine

[<Extension>]
type MyExtensions() =
    [<Extension>]
    static member WriteGiraffeView (ctx: HttpContext, htmlView: XmlNode) =
        let bytes = RenderView.AsBytes.htmlDocument htmlView
        ctx.SetContentType "text/html; charset=utf-8"
        ctx.WriteBytes bytes
```

### HTTP context extensions


| Giraffe                            | Oxpecker     |
|------------------------------------|--------------|
| `WriteBytesAsync`                  | `WriteBytes` |
| `WriteStringAsync`                 | _removed_    |
| `WriteTextAsync`                   | `WriteText`  |
| `WriteJsonAsync`                   | `WriteJson`  |
| `WriteJsonChunkedAsync`            | `WriteJsonChunked`  |
| `WriteXmlAsync`                    | _removed_    |
| `WriteHtmlFileAsync`               | _removed_    |
| `WriteHtmlStringAsync`             | `WriteHtmlString` |
| `WriteHtmlViewAsync `              | `WriteHtmlView` |
| `BindJsonAsync`                    | `BindJson`   |
| `BindXmlAsync`                     | _removed_    |
| `BindFormAsync`                    | `BindForm`   |
| `BindQueryString`                  | `BindQuery`  |
| `TryBindJsonAsync`                 | _removed_    |
| `TryBindXmlAsync`                  | _removed_    |
| `TryBindFormAsync`                 | _removed_    |
| `TryBindQueryString`               | _removed_    |
| `TryGetRouteValue`                 | `TryGetRouteValue` |
| `GetCookieValue `                  | `TryGetCookieValue` |
| `TryGetQueryStringValue`           | `TryGetQueryValue` |
| _missing_                          | `TryGetQueryValues` |
| `GetFormValue `                    | `TryGetFormValue` |
| _missing_                          | `TryGetFormValues` |
| `TryGetRequestHeader`              | `TryGetHeaderValue` |
| _missing_                          | `TryGetHeaderValues` |
| `GetXmlSerializer`                 |  _removed_ |
| `GetHostingEnvironment`            |  _removed_ |
| `ReadBodyFromRequestAsync`         |  _removed_ |
| `ReadBodyBufferedFromRequestAsync` |  _removed_ |

### HttpStatusCodeHandlers

The whole module was removed in favour of `IResult` integration

```fsharp
// Giraffe
Successful.OK myObject next ctx

// Oxpecker
ctx.Write <| TypedResults.Ok myObject
```

### routef parameters changes


| Format Char | Giraffe | Oxpecker                                                                                                                |
|-------------|---------|-------------------------------------------------------------------------------------------------------------------------|
| `%O`        | `Guid` (including short GUIDs*) | `Any object` (with [constraints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraints)) |
| `%u`        | `uint64` (formatted as a short ID*)  | `uint64` (regular format)                                                                                            |

Short ID and short GUID support was removed, however it could be added later as a `%O` custom constraint if needed.

### Content negotiation

Content negotiation was removed in Oxpecker.

### Response caching

Several helpers were removed for more flexibility

```fsharp
// Giraffe
responseCaching
    (Public (TimeSpan.FromSeconds (float 5)))
    (Some "Accept, Accept-Encoding")
    (Some [| "query1"; "query2" |])

// Oxpecker
responseCaching
    (Some <| CacheControlHeaderValue(MaxAge = TimeSpan.FromSeconds(5), Public = true))
    (Some "Accept, Accept-Encoding")
    (Some [| "query1"; "query2" |])
```

### Other changes

- `strOption` was dropped
- `readFileAsStringAsync` was dropped
- custom computation expressions for option and result were dropped


## Migrate from Endpoint Routing

### routef function

- Route parameters **must** now be enclosed in curly braces
- Route parameters are now curried

```fsharp
// Giraffe
routef "/hello/%s/%O" (fun (a, b) -> doSomething a b)

// Oxpecker
routef "/hello/{%s}/{%O:guid}" (fun a b -> doSomething a b)
```

### Setup

Here is the app and services configuration for Giraffe
```fsharp
// Giraffe
let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseGiraffe(endpoints)
        .UseGiraffe(notFoundHandler)

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddGiraffe() |> ignore
```

And here is the same configuration for Oxpecker
```fsharp
// Oxpecker
let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseOxpecker(endpoints)
        .Run(notFoundHandler)

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddOxpecker() |> ignore
```

## Migrate from Standard Routing

The main difference between Standard and Endpoint routing is that in Standard routing every route is tried out sequentially, while in Endpoint routing [all possible matches are processed at once](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#url-matching). Standard routing is essentially a sequential chain of monadic binds, while Endpoint routing is buiding a map of routes and letting `EndpointRouting` middleware handle it.

### Routing configuration

```fsharp
// Giraffe
let webApp =
    (choose [
        GET_HEAD >=> routef "/hello/%s" (fun name -> text $"Hello {name}!")
        GET >=> choose [
            route "/foo" >=> text "Bar"
            subRoute "/v2" >=> (choose [
                route "/foo" >=> text "Bar2"
            ])
        ]
    ])

// Oxpecker
let webApp = [
    GET_HEAD [ routef "/hello/{%s}" (fun name -> text $"Hello {name}!") ]
    GET [
        route "/foo" <| text "Bar"
        subRoute "/v2" [
            route "/foo" <| text "Bar2"
        ]
    ]
]
```

### Dropped functions

- `routeCi` (EndpointRouting has case-insensitive routing by default)
- `routex` (use regex [route constraints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-8.0#route-constraints) instead)
- `routexp`
- `routeCix`
- `routeCif`
- `routeBind`
- `routeStartsWith`
- `routeStartsWithCi`
- `subRouteCi`
- `subRoutef` (use subRoute + `TryGetRouteValue` extension method)
- `routePorts`

### Setup

Here is the app and services configuration for Giraffe
```fsharp
// Giraffe
let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseGiraffe(webApp)

let configureServices (services: IServiceCollection) =
    services
        .AddGiraffe() |> ignore
```
And here is the same configuration for Oxpecker
```fsharp
// Oxpecker
let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseOxpecker(endpoints)

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddOxpecker() |> ignore
```
