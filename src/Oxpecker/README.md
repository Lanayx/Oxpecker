# Oxpecker

Oxpecker is an F# framework based on ASP.NET Core Endpoint routing (similar to Minimal APIs, so they are competitors) with easy to comprehend API, mostly inherited from Giraffe framework.

[Nuget package](https://www.nuget.org/packages/Oxpecker)

Examples can be found [here](https://github.com/Lanayx/Oxpecker/tree/main/examples)

Performance tests reside [here](https://github.com/Lanayx/Oxpecker/tree/main/tests/PerfTest)

## Documentation:

An in depth functional reference to all of Oxpecker's features.

## Table of contents

- [Fundamentals](#fundamentals)
    - [Core concepts](#core-concepts)
    - [Oxpecker pipeline vs. ASP.NET Core pipeline](#oxpecker-pipeline-vs-aspnet-core-pipeline)
    - [Creating new EndpointHandler and EndpointMiddleware](#ways-of-creating-a-new-endpointhandler-and-endpointmiddleware)
    - [Composition](#composition)
    - [Continue vs. Return](#continue-vs-return)
- [Basics](#basics)
    - [Plugging Oxpecker into ASP.NET Core](#plugging-oxpecker-into-aspnet-core)
    - [Dependency Management](#dependency-management)
    - [Multiple Environments and Configuration](#multiple-environments-and-configuration)
    - [Logging](#logging)
    - [Error and NotFound handling](#error-handling)
- [Web Request Processing](#web-request-processing)
    - [HTTP Headers](#http-headers)
    - [HTTP Verbs](#http-verbs)
    - [HTTP Status Codes](#http-status-codes)
    - [Routing](#routing)
      - [route](#route)
      - [routef](#routef)
      - [subRoute](#subroute)
      - [addMetadata](#addmetadata)
      - [configureEndpoint](#configureendpoint)
    - [Query Strings](#query-strings)
    - [Model Binding](#model-binding)
      - [Binding JSON](#binding-json)
      - [Binding Forms](#binding-forms)
      - [Binding Query Strings](#binding-query-strings)
    - [File Upload](#file-upload)
    - [Authentication and Authorization](#authentication-and-authorization)
    - [Conditional Requests](#conditional-requests)
    - [Response Writing](#response-writing)
      - [Writing Bytes](#writing-bytes)
      - [Writing Text](#writing-text)
      - [Writing JSON](#writing-json)
      - [Writing IResult](#writing-iresult)
      - [Writing HTML strings](#writing-html-strings)
      - [Writing HTML views](#writing-html-views)
    - [Streaming](#streaming)
    - [Redirection](#redirection)
    - [Response compression](#response-compression)
- [Testing](#testing)

## Fundamentals

### Core concepts

Oxpecker is built on top of the ASP.NET Core [Endpoint Routing](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing) and provides some convenient DSL for F# users.

When using Oxpecker, make sure you are familiar with ASP.NET Core and it's concepts, since Oxpecker reuses a lot of built-in functionality.

#### EndpointHandler

The main building block in Oxpecker is an `EndpointHandler`:

```fsharp
type EndpointHandler = HttpContext -> Task
```

an `EndpointHandler` is a function which takes `HttpContext`, and returns a `Task` when finished.

`EndpointHandler` function has full control of the incoming `HttpRequest` and the resulting `HttpResponse`. It closely follows [RequestDelegate](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.requestdelegate) signature, but in F# style.

`EndpointHandler` normally should be regarded as a _terminal_ handler, meaning that it should write some result in response (but not necessary, as described in composition section).

#### EndpointMiddleware

```fsharp
type EndpointMiddleware = EndpointHandler -> HttpContext -> Task
```
`EndpointMiddleware` is similar to `EndpointHandler`, but accepts the _next_ `EndpointHandler` as first parameter.

Each `EndpointMiddleware` can process an incoming `HttpRequest` before passing it further down the Oxpecker pipeline by invoking the next `EndpointMiddleware` or short circuit the execution by returning the `Task` itself.

##### EndpointHandler vs EndpointMiddleware

So, when should you define one or another? The answer lies in the responsibility of your handler:
 - If you want to **conditionally** _return response_  or _proceed_ further in pipeline use `EndpointMiddleware`. Good example are [Auth endpoint middlewares](#authentication-and-authorization) and [Preconditional endpoint middleware](#conditional-requests)
 - If you want to execute some logic **after** the _next_ handler completes - use `EndpointMiddleware`
 - In other cases use `EndpointHandler`

### Oxpecker pipeline vs. ASP.NET Core pipeline

The Oxpecker pipeline is a (sort of) functional equivalent of the (object oriented) [ASP.NET Core pipeline](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware). The ASP.NET Core pipeline is defined by middlewares, and `EndpointMiddleware` is similar to regular middleware and `EndpointHandler` is similar to _terminal_ middleware.

If the Oxpecker pipeline didn't process an incoming `HttpRequest` (because no route was matched) then other ASP.NET Core middleware can still process the request (e.g. static file middleware or another web framework plugged in after Oxpecker).

This architecture allows F# developers to build rich web applications through a functional composition of `EndpointMiddleware` and `EndpointHandler` functions while at the same time benefiting from the wider ASP.NET Core eco system by making use of already existing ASP.NET Core middleware.

The Oxpecker pipeline is plugged into the wider ASP.NET Core pipeline through the `OxpeckerMiddleware` itself and therefore an addition to it rather than a replacement.

### Ways of creating a new EndpointHandler and EndpointMiddleware

There's multiple ways how one can create a new `EndpointHandler` in Oxpecker.

The easiest way is to re-use an existing `EndpointHandler` function:

```fsharp
let sayHelloWorld : EndpointHandler = text "Hello World, from Oxpecker"
```

You can also add additional parameters before returning an existing `EndpointHandler` function:

```fsharp
let sayHelloWorld (name: string) : EndpointHandler =
    let greeting = sprintf "Hello World, from %s" name
    text greeting
```

If you need to access the `HttpContext` object then you'll have to explicitly return an `EndpointHandler` function which accepts an `HttpContext` object and returns a `Task`:

```fsharp
let sayHelloWorld : EndpointHandler =
    fun (ctx: HttpContext) ->
        let name =
            ctx.TryGetQueryValue "name"
            |> Option.defaultValue "Oxpecker"
        let greeting = sprintf "Hello World, from %s" name
        text greeting ctx
```

The most verbose version of defining a new `EndpointHandler` function is by explicitly returning a `Task`. This is useful when an async operation needs to be called from within an `EndpointHandler` function:

```fsharp
type Person = { Name : string }

let sayHelloWorld : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let! person = ctx.BindJson<Person>()
            let greeting = sprintf "Hello World, from %s" person.Name
            return! text greeting ctx
        }
```

`EndpointMiddleware` is constructed very similarly to `EndpointHandler`, but it accepts an additional `EndpointHandler` as the first parameter:

```fsharp
let tryCatchMW : EndpointMiddleware =
    fun (next: EndpointHandler) (ctx: HttpContext) ->
        task {
            try
                return! next ctx
            with
            | ex ->
                ctx.Response.StatusCode <- 500
                return! text (sprintf "An error occurred: %s" ex.Message) ctx
        }
```


#### Deferred execution of Tasks

Please be also aware that a `Task<'T>` in .NET is just a promise of `'T` when a task eventually finishes asynchronously. Unless you define an `EndpointHandler` function in the most verbose way (with the `task {}` CE) and actively await a nested result with either `let!` or `return!` then the handler will not wait for the task to complete before returning to the `OxpeckerMiddleware`.

This has important implications if you want to execute code in an `EndpointHandler` after returned task completes, such as cleaning up resources with the `use` keyword. For example, in the code below, the `IDisposable` will get disposed **before** the actual response is returned. This is because a `EndpointHandler` is a `HttpContext -> Task` and therefore `text "Hello" ctx` only returns a `Task` which hasn't been completed yet:

```fsharp
let doSomething : EndpointHandler =
    fun ctx ->
        use __ = somethingToBeDisposedAtTheEndOfTheRequest
        text "Hello" ctx
```

However, by explicitly invoking the `text` from within a `task {}` CE one can ensure that the `text` gets executed before the `IDisposable` gets disposed:

```fsharp
let doSomething : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            use __ = somethingToBeDisposedAtTheEndOfTheRequest
            return! text "Hello" ctx
        }
```

### Composition

#### Handler composition

The fish operator (>=>) combines two functions into one.

It can compose

- `EndpointMiddleware` and `EndpointMiddleware`
- `EndpointMiddleware` and `EndpointHandler`
- `EndpointHandler` and `EndpointHandler`

It is an important combinator in Oxpecker which allows composing many smaller functions into a bigger web application:

There is no limit to how many functions can be chained with the fish operator:

```fsharp
let app =
    route "/" (
        setHttpHeader "X-Foo" "Bar"
        >=> setStatusCode 200
        >=> text "Hello World"
    )
```
The idea is that every function can decide: short-circuit pipeline or proceed. For `EndpointMiddleware` it's choice whether to call next or not, and for `EndpointHandler` it's to start writing a response or not.

If you would like to learn more about the origins of the `>=>` (fish) operator then please check out [Scott Wlaschin's blog post on Railway oriented programming](http://fsharpforfunandprofit.com/posts/recipe-part2/).

`routef` function doesn't work with fish operator directly, so additional operators where added for route readability

```fsharp
routef "/{%s}" (setStatusCode 200 >>=> handler)
routef "/{%s}/{%s}" (setStatusCode 200 >>=>+ handler)
routef "/{%s}/{%s}/{%s}" (setStatusCode 200 >>=>++ handler)
```
#### Bind composition

`bindQuery`, `bindForm`, `bindJson` helpers can be composed with handlers

```fsharp
route "/test" (bindQuery handler)
```
`routef` requires additional operators for composition with bind* functions as well
```fsharp
routef "/{%s}" (bindQuery << handler)
routef "/{%s}/{%s}" (bindForm <<+ handler)
routef "/{%s}/{%s}/{%s}" (bindJson <<++ handler)
```
#### Multi-route handler

Sometimes you want to use some generic handler or middleware not only with one route, but with the whole collection of routes. It is possible using `applyBefore` and `applyAfter` functions. For example:

```fsharp

let MY_HEADER = applyBefore (setHttpHeader "my" "header")

let webApp = [
    MY_HEADER <| subRoute "/auth" [
        route "/open" handler1
        route "/closed" handler2
    ]
]

```

### Continue vs. Return

In Oxpecker there are two scenarios which a given `EndpointMiddleware` or `EndpointHandler` can use:

- Continue with next handler
- Return early

#### Continue


An example is a hypothetical middleware, which sets a given HTTP header and afterwards always calls into the `next` http handler:

```fsharp
let setHttpHeader key value : EndpointMiddleware =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        ctx.SetHttpHeader key value
        next ctx
```
A middleware performs some actions on the `HttpRequest` and/or `HttpResponse` object and then invokes the `next` handler to **continue** with the pipeline.

It can also be implemented as an `EndpointHandler`:

```fsharp
let setHttpHeader key value : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.SetHttpHeader key value
        Task.CompletedTask
```
If such a handler is used in **the middle** of the pipeline, the next handler will be invoked, because the `ctx.Response.HasStarted` will return false.
If it will reside in **the end** of the pipeline, then the response will start anyway, since there's no next handler to be invoked.

#### Return early

Sometimes an `EndpointHandler` or `EndpointMiddleware` wants to return early and not continue with the remaining pipeline.

A typical example would be an authentication or authorization handler, which would not continue with the remaining pipeline if a user wasn't authenticated. Instead it might want to return a `401 Unauthorized` response:

```fsharp
let checkUserIsLoggedIn : EndpointMiddleware =
    fun (next: EndpointHandler) (ctx: HttpContext) ->
        if isNotNull ctx.User && ctx.User.Identity.IsAuthenticated then
            next ctx
        else
            setStatusCode 401 ctx
            Task.CompletedTask
```

In the `else` clause the `checkUserIsLoggedIn` handler returns a `401 Unauthorized` HTTP response and skips the remaining `EndpointHandler` pipeline by not invoking `next` but an already completed task.

If you were to have an `EndpointMiddleware` defined with the `task {}` CE then you could rewrite it in the following way:

```fsharp
let checkUserIsLoggedIn : EndpointMiddleware =
    fun (next: EndpointHandler) (ctx: HttpContext) ->
        task {
            if isNotNull ctx.User && ctx.User.Identity.IsAuthenticated then
                return! next ctx
            else
                return ctx.SetStatusCode 401
        }
```

It is also possible to implement this using `EndpointHandler`, however the response has to be explicitly started:

```fsharp
let checkUserIsLoggedIn : EndpointHandler =
    fun (ctx: HttpContext) ->
        if isNotNull ctx.User && ctx.User.Identity.IsAuthenticated then
            Task.CompletedTask
        else
            ctx.SetStatusCode 401
            text "Unauthorized" ctx // start response
```

## Basics

### Plugging Oxpecker into ASP.NET Core

Install the [Oxpecker](https://www.nuget.org/packages/Oxpecker) NuGet package:

```
PM> Install-Package Oxpecker
```

Create a web application and plug it into the ASP.NET Core middleware:

```fsharp
open Oxpecker

// usually your application consists of several routes
let webApp = [
    route "/"       <| text "Hello world"
    route "/ping"   <| text "pong"
]
// sometimes it can only be a single route
let webApp1 = route "/" <| "Hello Oxpecker"
// or it can only be a single "MultiEndpoint" route
let webApp2 = GET [
    route "/" <| "Hello Oxpecker"
]

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseOxpecker(webApp) // Add Oxpecker to the ASP.NET Core pipeline, should go after UseRouting
        //.UseOxpecker(webApp1) will work
        //.UseOxpecker(webApp2) will also work
    |> ignore

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddOxpecker() // Register default Oxpecker dependencies
    |> ignore

[<EntryPoint>]
let main _ =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0
```
### Dependency Management

ASP.NET Core has built in [dependency management](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) which works out of the box with Oxpecker.

#### Registering Services

[Registering services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#registering-services) is done the same way as it is done for any other ASP.NET Core web application:

```fsharp
let configureServices (services : IServiceCollection) =
    // Add default Oxpecker dependencies
    services.AddOxpecker() |> ignore

    // Add other dependencies
    // ...

```

#### Retrieving Services

Retrieving registered services from within a Oxpecker `EndpointHandler` function can be done through the built in service locator (`RequestServices`) which comes with an `HttpContext` object:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        let fooBar =
            ctx.RequestServices.GetService(typeof<IFooBar>)
            :?> IFooBar
        // Do something with `fooBar`...
        // Return a Task
```

Oxpecker has an additional `HttpContext` extension method called `GetService<'T>` to make the code less cumbersome:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        let fooBar = ctx.GetService<IFooBar>()
        // Do something with `fooBar`...
        // Return a Task
```

There's a handful more extension methods available to retrieve a few default dependencies like an `IWebHostEnvironment` or `ILogger` object which are covered in the respective sections of this document.


#### Functional DI
However, if you prefer to use a more functional approach to dependency injection, you shouldn't use container based approach, but rather follow the _Env_ strategy.

The approach is described in the article https://medium.com/@lanayx/dependency-injection-in-f-the-missing-manual-d376e9cafd0f , and to see how it looks in practice, you can refer to the [CRUD example in the repository.](https://github.com/Lanayx/Oxpecker/tree/main/examples/CRUD)



### Multiple Environments and Configuration

ASP.NET Core has built in support for [working with multiple environments](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments) and [configuration management](https://docs.microsoft.com/en-gb/aspnet/core/fundamentals/configuration), which both work out of the box with Oxpecker.

Additionally Oxpecker exposes a `GetHostingEnvironment()` extension method which can be used to easier retrieve an `IWebHostEnvironment` object from within an `EndpointHandler` function:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        let env = ctx.GetHostingEnvironment()
        // Do something with `env`...
        // Return a Task
```

Configuration options can be retrieved via the `GetService<'T>` extension method:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        let settings = ctx.GetService<IOptions<MySettings>>()
        // Do something with `settings`...
        // Return a Task
```

If you need to access the configuration when configuring services, you can access it like this:

```fsharp
let configureServices (services: IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()
    // Configure services using the `settings`...
    services.AddOxpecker() |> ignore
```
### Logging

ASP.NET Core has a built in [Logging API](https://docs.microsoft.com/en-gb/aspnet/core/fundamentals/logging) which works out of the box with Oxpecker.


#### Logging from within an EndpointHandler function

You can retrieve an `ILogger` object (which can be used for logging) through the `GetLogger<'T>()` or `GetLogger (categoryName : string)` extension methods:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        // Retrieve an ILogger through one of the extension methods
        let loggerA = ctx.GetLogger<ModuleName>()
        let loggerB = ctx.GetLogger("someHandler")

        // Log some data
        loggerA.LogCritical("Something critical")
        loggerB.LogInformation("Logging some random info")
        // etc.

        // Return a Task
```

### Error Handling

Oxpecker doesn't have a built in error handling or not found handling mechanisms, since it can be easily implemented using following functions that should be registered before and after the Oxpecker middleware:

```fsharp
// error handling middleware
let errorHandler (ctx: HttpContext) (next: RequestDelegate) =
    task {
        try
            return! next.Invoke(ctx)
        with
        | :? ModelBindException
        | :? RouteParseException as ex ->
            let logger = ctx.GetLogger()
            logger.LogWarning(ex, "Unhandled 400 error")
            ctx.SetStatusCode StatusCodes.Status400BadRequest
            return! ctx.WriteHtmlView(errorView 400 (string ex))
        | ex ->
            let logger = ctx.GetLogger()
            logger.LogError(ex, "Unhandled 500 error")
            ctx.SetStatusCode StatusCodes.Status500InternalServerError
            return! ctx.WriteHtmlView(errorView 500 (string ex))
    } :> Task

// not found terminal middleware
let notFoundHandler (ctx: HttpContext) =
    let logger = ctx.GetLogger()
    logger.LogWarning("Unhandled 404 error")
    ctx.SetStatusCode 404
    ctx.WriteHtmlView(errorView 404 "Page not found!")

///...

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .Use(errorHandler) // Add error handling middleware BEFORE Oxpecker
        .UseOxpecker(endpoints)
        .Run(notFoundHandler) // Add not found middleware AFTER Oxpecker
```

## Web Request Processing

Oxpecker comes with a large set of default `HttpContext` extension methods as well as default `EndpointHandler` functions which can be used to build rich web applications.

### HTTP Headers

Working with HTTP headers in Oxpecker is plain simple. The `TryGetHeaderValue (key: string)` extension method tries to retrieve the value of a given HTTP header and then returns either `Some string` or `None`:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        let someValue =
            match ctx.TryGetHeaderValue "X-MyOwnHeader" with
            | None -> "default value"
            | Some headerValue -> headerValue

        // Do something with `someValue`...
        // Return a Task
```

Setting an HTTP header in the response can be done via the `SetHttpHeader (key: string) (value: obj)` extension method:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.SetHttpHeader "X-CustomHeader" "some-value"
        // Do other stuff...
        // Return a Task
```

You can also set an HTTP header via the `setHttpHeader` http handler:

```fsharp
let customHeader : EndpointHandler =
    setHttpHeader "X-CustomHeader" "Some value"

let webApp = [
    route "/foo" (customHeader >=> text "Foo")
]
```

Please note that these are additional Oxpecker functions which complement already existing HTTP header functionality in the ASP.NET Core framework. ASP.NET Core offers higher level HTTP header functionality through the `ctx.Request.GetTypedHeaders()` method.

### HTTP Verbs

Oxpecker exposes a set of functions which can filter a request based on the request's HTTP verb:

- `GET`
- `POST`
- `PUT`
- `PATCH`
- `DELETE`
- `HEAD`
- `OPTIONS`
- `TRACE`
- `CONNECT`

There is an additional `GET_HEAD` handler which can filter an HTTP `GET` and `HEAD` request at the same time.

Filtering requests based on their HTTP verb can be useful when implementing a route which should behave differently based on the verb (e.g. `GET` vs. `POST`):

```fsharp
let submitFooHandler : EndpointHandler =
    // Do something

let submitBarHandler : EndpointHandler =
    // Do something

let webApp = [
    // Filters for GET requests
    GET [
        route "/foo" <| text "Foo"
        route "/bar" <| text "Bar"
    ]
    // Filters for POST requests
    POST [
        route "/foo" <| submitFooHandler
        route "/bar" <| submitBarHandler
    ]
]
```

If you need to check the request's HTTP verb from within an `EndpointHandler` function then you can use the default ASP.NET Core `HttpMethods` class:

```fsharp
open Microsoft.AspNetCore.Http

let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        if HttpMethods.IsPut ctx.Request.Method then
            // Do something
        else
            // Do something else
        // Return a Task
```

The `GET_HEAD` is a special function which can be used to enable `GET` and `HEAD` requests on a resource at the same time. This can be very useful when caching is enabled and clients might want to send `HEAD` requests to check the `ETag` or `Last-Modified` HTTP headers before issuing a `GET`.

You can also create custom combinations of HTTP verbs by using the `applyHttpVerbsToEndpoints` function:

```fsharp
let GET_HEAD_OPTIONS: Endpoint seq -> Endpoint =
    applyHttpVerbsToEndpoints(Verbs [ HttpVerb.GET; HttpVerb.HEAD; HttpVerb.OPTIONS ])

let webApp = [
    GET_HEAD_OPTIONS [
        route "/foo" <| text "Foo"
        route "/bar" <| text "Bar"
    ]
]
```


### HTTP Status Codes

Setting the HTTP status code of a response can be done either via the `SetStatusCode (httpStatusCode: int)` extension method or with the `setStatusCode (statusCode: int)` function:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.SetStatusCode 200
        // Return a Task

// or...

let someHandler : EndpointHandler =
    setStatusCode 200
    >=> text "Hello World"
```

### Routing

Oxpecker offers several routing functions to accommodate the majority of use cases. Note, that Oxpecker routing is sitting on the top of ASP.NET Core endpoint routing, so all routes are case insensitive.

#### route

The simplest form of routing can be done with the `route` http handler:

```fsharp
let webApp = [
    route "/foo" <| text "Foo"
    route "/bar" <| text "Bar"
]
```

#### routef

If a route contains user defined parameters then the `routef` http handler can be handy:

```fsharp
let fooHandler first last age : EndpointHandler =
    fun (ctx: HttpContext) ->
        (sprintf "First: %s, Last: %s, Age: %i" first last age
        |> text) ctx

let webApp = [
    routef "/foo/{%s}/{%s}/{%i}" fooHandler
    routef "/bar/{%O:guid}" (fun (guid: Guid) -> text (string guid))
]
```

The `routef` http handler takes two parameters - a format string and an `EndpointHandler` function.

The format string supports the following format chars:

| Format Char | Type                                                                                                                    |
|-------------|-------------------------------------------------------------------------------------------------------------------------|
| `%b`        | `bool`                                                                                                                  |
| `%c`        | `char`                                                                                                                  |
| `%s`        | `string`                                                                                                                |
| `%i`        | `int`                                                                                                                   |
| `%d`        | `int64`                                                                                                                 |
| `%f`        | `float`/`double`                                                                                                        |
| `%u`        | `uint64`                                                                                                                |
| `%O`        | `Any object` (with [constraints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraints)) |

_Note_: `routef` handler can only handle up to 5 route parameters. It's not recommended to use more than 3 parameters in a route, but if you really need a lot, you can use `route` with `EndpointHandler` function that utilizes `.TryGetRouteValue` extension.

```fsharp
route "/{a}/{b}/{c}/{d}/{e}/{f}" (fun ctx ->
    let a = ctx.TryGetRouteValue("a") |> Option.defaultValue ""
    let b = ctx.TryGetRouteValue("b") |> Option.defaultValue ""
    let c = ctx.TryGetRouteValue("c") |> Option.defaultValue ""
    let d = ctx.TryGetRouteValue("d") |> Option.defaultValue ""
    let e = ctx.TryGetRouteValue("e") |> Option.defaultValue ""
    let f = ctx.TryGetRouteValue("f") |> Option.defaultValue ""
    text (sprintf "%s %s %s %s %s %s", a b c d e f) ctx
)
```

#### subRoute

It lets you categorise routes without having to repeat already pre-filtered parts of the route:

```fsharp
let webApp =
    subRoute "/api" [
        subRoute "/v1" [
            route "/foo" <| text "Foo 1"
            route "/bar" <| text "Bar 1"
        ]
        subRoute "/v2" [
            route "/foo" <| text "Foo 2"
            route "/bar" <| text "Bar 2"
        ]
    ]
```

In this example the final URL to retrieve "Bar 2" would be `http[s]://your-domain.com/api/v2/bar`.

#### addMetadata

It lets you add [metadata](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#endpoint-metadata) to a route which can be used later on in the pipeline:

```fsharp
let webApp =
    GET [
        route "/foo" (text "Foo") |> addMetadata "foo"
    ]
```

#### configureEndpoint

This function allows you to configure an endpoint using ASP.NET `.With*` extension methods:

```fsharp
let webApp =
    GET [
        route "/foo" (text "Foo")
          |> configureEndpoint
             _.WithMetadata("foo")
              .WithDisplayName("Foo")
    ]
```

### Query Strings

Working with query strings is very similar to working with HTTP headers in Oxpecker. The `TryGetQueryValue (key : string)` extension method tries to retrieve the value of a given query string parameter and then returns either `Some string` or `None`:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        let someValue =
            match ctx.TryGetQueryValue "q" with
            | None   -> "default value"
            | Some q -> q

        // Do something with `someValue`...
        // Return a Task
```

You can also access the query string through the `ctx.Request.Query` object which returns an `IQueryCollection` object which allows you to perform more actions on it.

Last but not least there is also an `HttpContext` extension method called `BindQuery<'T>` which lets you bind an entire query string to an object of type `'T` (see [Binding Query Strings](#binding-query-strings)).

### Model Binding

Oxpecker offers out of the box a few default `HttpContext` extension methods and equivalent `EndpointHandler` functions which make it possible to bind the payload or query string of an HTTP request to a custom object.

#### Binding JSON

The `BindJson<'T>()` extension method can be used to bind a JSON payload to an object of type `'T`:

```fsharp
[<CLIMutable>]
type Car = {
    Name   : string
    Make   : string
    Wheels : int
    Built  : DateTime
}

let submitCar : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Binds a JSON payload to a Car object
            let! car = ctx.BindJson<Car>()

            // Sends the object back to the client
            return! ctx.Write <| TypedResults.Ok car
        }

let webApp = [
    GET [
        route "/"    <| text "index"
        route "ping" <| text "pong"
    ]
    POST [
        route "/car" submitCar
    ]
]
```

Alternatively you can also use the `bindJson<'T>` http handler:

```fsharp
[<CLIMutable>]
type Car = {
    Name   : string
    Make   : string
    Wheels : int
    Built  : DateTime
}

let webApp = [
    GET [
        route "/"    <| text "index"
        route "ping" <| text "pong"
    ]
    POST [
        route "/car" (bindJson<Car> (fun car -> %TypedResults.Ok car))
    ]
]
```

Both, the `HttpContext` extension method as well as the `EndpointHandler` function will try to create an instance of type `'T` regardless if the submitted payload contained a complete representation of `'T` or not. The parsed object might only contain partial data (where some properties might be `null`) and additional `null` checks might be required before further processing.

Please note that in order for the model binding to work the record type must be decorated with the `[<CLIMutable>]` attribute, which will make sure that the type will have a parameterless constructor.

The underlying JSON serializer can be configured as a dependency during application startup (see [JSON](#writing-json)).

#### Binding Forms

The `BindForm<'T> (?cultureInfo : CultureInfo)` extension method binds form data to an object of type `'T`. You can also specify an optional `CultureInfo` object for parsing culture specific data such as `DateTime` objects or floating point numbers:

```fsharp
[<CLIMutable>]
type Car = {
    Name   : string
    Make   : string
    Wheels : int
    Built  : DateTime
}

let submitCar : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Binds a form payload to a Car object
            let! car = ctx.BindForm<Car>()

            // or with a CultureInfo:
            let british = CultureInfo.CreateSpecificCulture("en-GB")
            let! car2 = ctx.BindForm<Car>(british)

            // Sends the object back to the client
            return! ctx.Write <| Ok car
        }

let webApp = [
    GET [
        route "/"    <| text "index"
        route "ping" <| text "pong"
    ]
    POST [ route "/car" submitCar ]
]
```

Alternatively you can use the `bindForm<'T>` and `bindFormC<'T>`(additional culture parameter) http handlers:

```fsharp
[<CLIMutable>]
type Car = {
    Name   : string
    Make   : string
    Wheels : int
    Built  : DateTime
}

let british = CultureInfo.CreateSpecificCulture("en-GB")

let webApp = [
    GET [
        route "/"    <| text "index"
        route "ping" <| text "pong"
    ]
    POST [
        route "/car" (bindForm<Car> (fun model -> %Ok model))
        route "/britishCar" (bindFormC<Car> british (fun model -> %Ok model))
    ]
]
```

Just like in the previous examples the record type must be decorated with the `[<CLIMutable>]` attribute in order for the model binding to work.

#### Binding Query Strings

The `BindQuery<'T> (?cultureInfo: CultureInfo)` extension method binds query string parameters to an object of type `'T`. An optional `CultureInfo` object can be specified for parsing culture specific data such as `DateTime` objects and floating point numbers:

```fsharp
[<CLIMutable>]
type Car = {
    Name   : string
    Make   : string
    Wheels : int
    Built  : DateTime
}

let submitCar : EndpointHandler =
    fun (ctx: HttpContext) ->
        // Binds the query string to a Car object
        let car = ctx.BindQuery<Car>()

        // or with a CultureInfo:
        let british = CultureInfo.CreateSpecificCulture("en-GB")
        let car2 = ctx.BindQuery<Car>(british)

        // Sends the object back to the client
        ctx.Write <| Ok car

let webApp = [
    GET [
        route "/"    <| text "index"
        route "ping" <| text "pong"
        route "/car" <| submitCar
    ]
]
```

Alternatively you can use the `bindQuery<'T>` and `bindQueryC<'T>`(additional culture parameter) http handlers:

```fsharp
[<CLIMutable>]
type Car = {
    Name   : string
    Make   : string
    Wheels : int
    Built  : DateTime
}

let british = CultureInfo.CreateSpecificCulture("en-GB")

let webApp = [
    GET [
        route "/"    <| text "index"
        route "ping" <| text "pong"
    ]
    POST [
        route "/car" (bindQuery<Car> (fun model -> %Ok model))
        route "/britishCar" (bindQueryC<Car> british (fun model -> %Ok model))
    ]
]
```

Just like in the previous examples the record type must be decorated with the `[<CLIMutable>]` attribute in order for the model binding to work.

### File Upload

ASP.NET Core makes it really easy to process uploaded files.

The `HttpContext.Request.Form.Files` collection can be used to process one or many small files which have been sent by a client:

```fsharp
let fileUploadHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        match ctx.Request.HasFormContentType with
        | false ->
            ctx.Write <| BadRequest()
        | true  ->
            ctx.Request.Form.Files
            |> Seq.fold (fun acc file -> $"{acc}\n{file.FileName}") ""
            |> ctx.WriteText

let webApp = [ route "/upload" fileUploadHandler ]
```

You can also read uploaded files by utilizing the `IFormFeature` and the `ReadFormAsync` method:

```fsharp
let fileUploadHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let formFeature = ctx.Features.Get<IFormFeature>()
            let! form = formFeature.ReadFormAsync CancellationToken.None
            return!
                form.Files
                |> Seq.fold (fun acc file -> $"{acc}\n{file.FileName}") ""
                |> ctx.WriteText
        }

let webApp = [ route "/upload" fileUploadHandler ]
```

For large file uploads it is recommended to [stream the file](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads#upload-large-files-with-streaming) in order to prevent resource exhaustion.

See also [large file uploads in ASP.NET Core](https://stackoverflow.com/questions/36437282/dealing-with-large-file-uploads-on-asp-net-core-1-0) on StackOverflow.

### Authentication and Authorization

Oxpecker's security model is the same as [Minimal API security model](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security), please make sure you are very familiar with it.
The main difference is that in Oxpecker you can conveniently call `configureEndpoint _.RequireAuthorization` on both a single endpoint and a group of endpoints.
```fsharp
let webApp = [
    // single endpoint
    route "/" (text "Hello World")
        |> configureEndpoint
            _.DisableAntiforgery()
             .RequireAuthorization()
    // endpoint group
    GET [
        route "/index" <| text "index"
        route "/ping"  <| text "pong"
    ] |> configureEndpoint _.RequireAuthorization(
            AuthorizeAttribute(AuthenticationSchemes = "MyScheme")
        )
]
```
### Conditional Requests

Conditional HTTP headers (e.g. `If-Match`, `If-Modified-Since`, etc.) are a common pattern to improve performance (web caching), to combat the [lost update problem](https://www.w3.org/1999/04/Editing/) or to perform [optimistic concurrency control](https://en.wikipedia.org/wiki/Optimistic_concurrency_control) when a client requests a resource from a web server.

Oxpecker offers the `validatePreconditions` endpoint handler which can be used to run HTTP pre-validation checks against a given `ETag` and/or `Last-Modified` value of an incoming HTTP request:

```fsharp
let someHandler (eTag         : string)
                (lastModified : DateTimeOffset)
                (content      : string) =
    let eTagHeader = Some (EntityTagHelper.createETag eTag)
    validatePreconditions eTagHeader (Some lastModified) >=> text content
```

The `validatePreconditions` middleware takes in two optional parameters - an `eTag` and a `lastMofified` date time value - which will be used to validate a conditional HTTP request. If all conditions can be met, or if no conditions have been submitted, then the `next` http handler (of the Oxpecker pipeline) will get invoked. Otherwise, if one of the pre-conditions fails or if the resource hasn't changed since the last check, then a `412 Precondition Failed` or a `304 Not Modified` response will get returned.

The [ETag (Entity Tag)](https://tools.ietf.org/html/rfc7232#section-2.3) value is an opaque identifier assigned by a web server to a specific version of a resource found at a URL. The [Last-Modified](https://tools.ietf.org/html/rfc7232#section-2.2) value provides a timestamp indicating the date and time at which the origin server believes the selected representation was last modified.

Oxpecker's `validatePreconditions` endpoint middleware validates the following conditional HTTP headers:

- `If-Match`
- `If-None-Match`
- `If-Modified-Since`
- `If-Unmodified-Since`

The `If-Range` HTTP header will not get validated as part the `validatePreconditions` http handler, because it is a streaming specific check which gets handled by Oxpecker's [Streaming](#streaming) functionality.

Alternatively Oxpecker exposes the `HttpContext` extension method `ValidatePreconditions(eTag, lastModified)` which can be used to create a custom conditional endpoint middleware. The `ValidatePreconditions` method takes the same two optional parameters and returns a result of type `Precondition`.

The `Precondition` union type contains the following cases:

| Case                    | Description and Recommended Action                                                                                                                                                                                             |
|-------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `NoConditionsSpecified` | No validation has taken place, because the client didn't send any conditional HTTP headers. Proceed with web request as normal.                                                                                                |
| `ConditionFailed`       | At least one condition couldn't be satisfied. It is advised to return a `412` status code back to the client (you can use the `HttpContext.PreconditionFailedResponse()` method for that purpose).                             |
| `ResourceNotModified`   | The resource hasn't changed since the last visit. The server can skip processing this request and return a `304` status code back to the client (you can use the `HttpContext.NotModifiedResponse()` method for that purpose). |
| `AllConditionsMet`      | All pre-conditions were satisfied. The server should continue processing the request as normal.                                                                                                                                |

The `validatePreconditions` http handler as well as the `ValidatePreconditions` extension method will not only validate all conditional HTTP headers, but also set the required `ETag` and/or `Last-Modified` HTTP response headers according to the HTTP spec.

Both functions follow latest HTTP guidelines and validate all conditional headers in the correct precedence as defined in [RFC 2616](https://tools.ietf.org/html/rfc2616).

Example of `HttpContext.ValidatePreconditions`:

```fsharp
// Pass an optional eTag and lastModified timestamp into the handler, because generating an eTag might require to load the entire resource into memory and therefore this is not something which should be done on every request.
let someHttpHandler eTag lastModified : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            match ctx.ValidatePreconditions(eTag, lastModified) with
            | ConditionFailed     -> return ctx.PreconditionFailedResponse()
            | ResourceNotModified -> return ctx.NotModifiedResponse()
            | AllConditionsMet | NoConditionsSpecified ->
                // Continue as normal
                // Do stuff
        }

let webApp = [
    route "/"    <| text "Hello World"
    route "/foo" <| someHttpHandler None None
]
```
### Response Writing

Sending a response back to a client in Oxpecker can be done through a small range of `HttpContext` extension methods and their equivalent `EndpointHandler` functions.

#### Writing Bytes

The `WriteBytes (data: byte[])` extension method and the `bytes (data: byte[])` endpoint handler both write a `byte array` to the response stream of the HTTP request:

```fsharp
let someHandler (data: byte[]) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Do stuff
            return! ctx.WriteBytes data
        }

// or...

let someHandler (data: byte[]) : EndpointHandler =
    // Do stuff
    bytes data
```

Both functions will also set the `Content-Length` HTTP header to the length of the `byte array`.

The `bytes` http handler (and it's `HttpContext` extension method equivalent) is useful when you want to create your own response writing function for a specific media type which is not provided by Oxpecker yet.

For example Oxpecker doesn't have any functionality for serializing and writing a YAML response back to a client. However, you can reference another third party library which can serialize an object into a YAML string and then create your own `yaml` http handler like this:

```fsharp
let yaml (x: obj) : EndpointHandler =
    setHttpHeader "Content-Type" "text/yaml"
    >=> bytes (x |> YamlSerializer.toYaml |> Encoding.UTF8.GetBytes)

```

#### Writing Text

The `WriteText (str : string)` extension method and the `text (str: string)` endpoint handler will write string to response in UTF8 format and also set the `Content-Type` HTTP header to `text/plain` in the response:

```fsharp
let someHandler (str: string) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Do stuff
            return! ctx.WriteText str
        }

// or...

let someHandler (str: string) : EndpointHandler =
    // Do stuff
    text str
```

#### Writing JSON

The `WriteJson<'T> (dataObj : 'T)` extension method and the `json<'T> (dataObj: 'T)` endpoint handler will both serialize an object to a JSON string and write the output to the response stream of the HTTP request. They will also set the `Content-Length` HTTP header and the `Content-Type` header to `application/json` in the response:

```fsharp
let someHandler (animal: Animal) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Do stuff
            return! ctx.WriteJson animal
        }

// or...

let someHandler (animal: Animal) : EndpointHandler =
    // Do stuff
    json animal
```

The `WriteJsonChunked<'T> (dataObj: 'T)` extension method and the `jsonChunked (dataObj: 'T)` endpoint handler write directly to the response stream of the HTTP request without extra buffering into a byte array. They will not set a `Content-Length` header and instead set the `Transfer-Encoding: chunked` header and `Content-Type: application/json`:

```fsharp
let someHandler (person: Person) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Do stuff
            return! ctx.WriteJsonChunked person
        }

// or...

let someHandler (person: Person) : EndpointHandler =
    // Do stuff
    jsonChunked person
```

The underlying JSON serializer is configured as a dependency during application startup and defaults to `System.Text.Json` (when you write `services.AddOxpecker()`). You can implement `Serializers.IJsonSerializer` interface to plug in custom JSON serializer.

```fsharp
let configureServices (services : IServiceCollection) =
    // First register all default Oxpecker dependencies
    services.AddOxpecker() |> ignore
    // Now register custom serializer
    services.AddSingleton<Serializers.IJsonSerializer>(CustomSerializer()) |> ignore
    // or use default STJ serializer, but with different options
    services.AddSingleton<Serializers.IJsonSerializer>(
            SystemTextJson.Serializer(specificOptions)) |> ignore
```

#### Writing IResult

If you like what ASP.NET Core IResult offers, you might be pleased to know that Oxpecker supports it as well. You can simplify returning responses together with status codes using `Microsoft.AspNetCore.Http.TypedResults`:

```fsharp
open Oxpecker
open type Microsoft.AspNetCore.Http.TypedResults

let johnDoe = {|
    FirstName = "John"
    LastName  = "Doe"
|}

let app = [
    route "/"     <| text "Hello World"
    route "/john" <| %Ok johnDoe // returns 200 OK with JSON body
    route "/bad"  <| %BadRequest() // returns 400 BadRequest with empty body
]
```
The `%` operator is used to convert `IResult` to `EndpointHandler`. You can also do the conversion inside EndpointHandler using `.Write` extension method:

```fsharp
let myHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.Write <| TypedResults.Ok johnDoe
```

#### Writing HTML Strings

The `WriteHtmlString (html: string)` extension method and the `htmlString (html: string)` endpoint handler are both equivalent to [writing text](#writing-text) except that they set the `Content-Type` header to `text/html`:

```fsharp
let someHandler (dataObj: obj) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Do stuff
            return! ctx.WriteHtmlString "<html><head></head><body>Hello World</body></html>"
        }

// or...

let someHandler (dataObj: obj) : EndpointHandler =
    // Do stuff
    htmlString "<html><head></head><body>Hello World</body></html>"
```

#### Writing HTML Views

Oxpecker comes with its own extremely powerful view engine for functional developers (see [Oxpecker View Engine](https://github.com/Lanayx/Oxpecker/blob/main/src/Oxpecker.ViewEngine/README.md)). The `WriteHtmlView (htmlView : HtmlElement)` extension method and the `htmlView (htmlView : HtmlElement)` http handler will both compile a given html view into valid HTML code and write the output to the response stream of the HTTP request. Additionally they will both set the `Content-Length` HTTP header to the correct value and set the `Content-Type` header to `text/html`:

```fsharp
let indexView =
    html() {
        head() {
            title() { "Oxpecker" }
        }
        body() {
            h1(id="Header") { "Oxpecker" }
            p() { "Hello World." }
        }
    }

let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Do stuff
            return! ctx.WriteHtmlView indexView
        }

// or...

let someHandler : EndpointHandler =
    // Do stuff
    htmlView indexView
```

**Warning**: While being fast at runtime, using many long CE expressions might slow down your project compilation and IDE experience (see [the issue](https://github.com/Lanayx/Oxpecker/issues/5)), so you might decide to use a different view engine. There are multiple view engines for your choice: Giraffe.ViewEngine, Feliz.ViewEngine, Falco.Markup or you can even write your own! To plug in an external view engine you can write a simple extension:
```fsharp
[<Extension>]
static member WriteMyHtmlView(ctx: HttpContext, htmlView: MyHtmlElement) =
    let bytes = htmlView |> convertToBytes
    ctx.Response.ContentType <- "text/html; charset=utf-8"
    ctx.WriteBytes bytes

// ...

let myHtmlView (htmlView: MyHtmlElement) : EndpointHandler =
    fun (ctx: HttpContext) -> ctx.WriteMyHtmlView htmlView
```

### Streaming

Sometimes a large file or block of data has to be send to a client and in order to avoid loading the entire data into memory a Oxpecker web application can use streaming to send a response in a more efficient way.

The `WriteStream` extension method and the `streamData` endpoint handler can be used to stream an object of type `Stream` to a client.

Both functions accept the following parameters:

- `enableRangeProcessing`: If true a client can request a sub range of data to be streamed (useful when a client wants to continue streaming after a paused download, or when internet connection has been lost, etc.)
- `stream`: The stream object to be returned to the client.
- `eTag`: Entity header tag used for conditional requests (see [Conditional Requests](#conditional-requests)).
- `lastModified`: Last modified timestamp used for conditional requests (see [Conditional Requests](#conditional-requests)).

If the `eTag` or `lastModified` timestamp are set then both functions will also set the `ETag` and/or `Last-Modified` HTTP headers during the response:

```fsharp
let someStream : Stream = ...

let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Do stuff
            return! ctx.WriteStream(
                true, // enableRangeProcessing
                someStream,
                None, // eTag
                None) // lastModified
        }

// or...

let someHandler : EndpointHandler =
    // Do stuff
    streamData
        true // enableRangeProcessing
        someStream
        None // eTag
        None // lastModified
```

In most cases a web application will want to stream a file directly from the local file system. In this case you can use the `WriteFileStream` extension method or the `streamFile` http handler, which are both the same as `WriteStream` and `streamData` except that they accept a relative or absolute `filePath` instead of a `Stream` object:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Do stuff
            return! ctx.WriteFileStream(
                true, // enableRangeProcessing
                "large-file.zip",
                None, // eTag
                None) // lastModified
        }

// or...

let someHandler : EndpointHandler =
    // Do stuff
    streamFile
        true // enableRangeProcessing
        "large-file.zip"
        None // eTag
        None // lastModified
```

All streaming functions in Oxpecker will also validate conditional HTTP headers, including the `If-Range` HTTP header if `enableRangeProcessing` has been set to `true`.

### Redirection

The `redirectTo (location: string) (permanent: bool)` endpoint handler can be used to redirect a client to a different location when handling an incoming web request:

```fsharp
let webApp = [
    route "/new" <| text "Hello World"
    route "/old" <| redirectTo "https://myserver.com/new" true
]
```

Please note that if the `permanent` flag is set to `true` then the Oxpecker web application will send a `301` HTTP status code to browsers which will tell them that the redirection is permanent. This often leads to browsers cache the information and not hit the deprecated URL a second time any more. If this is not desired then please set `permanent` to `false` (`302` HTTP status code) in order to guarantee that browsers will continue hitting the old URL before redirecting to the (temporary) new one.

### Response Caching

ASP.NET Core comes with a standard [Response Caching Middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/middleware) which works out of the box with Oxpecker.

If you are not already using one of the two ASP.NET Core meta packages (`Microsoft.AspNetCore.App` or `Microsoft.AspNetCore.All`) then you will have to add an additional reference to the [Microsoft.AspNetCore.ResponseCaching](https://www.nuget.org/packages/Microsoft.AspNetCore.ResponseCaching/) NuGet package.

After adding the NuGet package you need to register the response caching middleware inside your application's startup code before registering Oxpecker:

```fsharp
let configureServices (services : IServiceCollection) =
    services
        .AddResponseCaching() // <-- Here the order doesn't matter
        .AddOxpecker()         // This is just registering dependencies
    |> ignore

let configureApp (app : IApplicationBuilder) =
    app
       .UseStaticFiles()     // Optional if you use static files
       .UseAuthentication()  // Optional if you use authentication
       .UseResponseCaching() // <-- Before UseOxpecker webApp
       .UseOxpecker webApp
```

After setting up the [ASP.NET Core response caching middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/middleware#configuration) you can use Oxpecker's response caching http handlers to add response caching to your routes:

```fsharp
// A test handler which generates a new GUID on every request
let generateGuidHandler : EndpointHandler =
    fun ctx -> ctx.WriteText(Guid.NewGuid().ToString())

let cacheHeader = Some <| CacheControlHeaderValue(MaxAge = TimeSpan.FromSeconds(30), Public = true)

let webApp = [
    route "/route1" (responseCaching cacheHeader None None >=> generateGuidHandler)
    route "/route2" (noResponseCaching >=> generateGuidHandler)
]
```

Requests to `/route1` can be cached for up to 30 seconds whilst requests to `/route2` have response caching completely disabled.

*Note: if you test the above code with [Postman](https://www.getpostman.com/) then make sure you [disable the No-Cache feature](https://www.getpostman.com/docs/v6/postman/launching_postman/settings) in Postman in order to test the correct caching behaviour.*

Oxpecker offers in total 2 endpoint handlers which can be used to configure response caching for an endpoint.

In the above example we used the `noResponseCaching` endpoint handler to completely disable response caching on the client and on any proxy server. The `noResponseCaching` endpoint handler will send the following HTTP headers in the response:

```
Cache-Control: no-store, no-cache
Pragma: no-cache
Expires: -1
```

The `responseCaching` endpoint handler will enable response caching on the client and/or on proxy servers. The
`CacheControlHeaderValue` object will control the `Cache-Control` directive.

`Public = true` means that not only the client is allowed to cache a response for the given cache duration, but also any intermediary proxy server as well as the ASP.NET Core middleware. This is useful for HTTP GET/HEAD endpoints which do not hold any user specific data, authentication data or any cookies and where the response data doesn't change frequently.

`Public = false` which means that only the end client is allowed to store the response for the given cache duration. Proxy servers and the ASP.NET Core response caching middleware must not cache the response.

The `responseCaching` endpoint handler has two additional parameters: `vary` and `varyByQueryKeys`.

#### Vary

The `vary` parameter specifies which HTTP request headers must be respected to vary the cached response. For example if an endpoint returns a different response (`Content-Type`) based on the client's `Accept` header (content negotiation) then the `Accept` header must also be considered when returning a response from the cache. The same applies if the web server has response compression enabled. If a response varies based on the client's accepted compression algorithm then the cache must also respect the client's `Accept-Encoding` HTTP header when serving a response from the cache.

```fsharp
let cacheHeader = Some <| CacheControlHeaderValue(MaxAge = TimeSpan.FromSeconds(30), Public = true)

// Cache for 30 seconds without any vary headers
publicResponseCaching cacheHeader None None

// Cache for 30 seconds with Accept and Accept-Encoding as vary headers
publicResponseCaching cacheHeader (Some "Accept, Accept-Encoding") None
```

#### VaryByQueryKeys

The ASP.NET Core response caching middleware offers one more additional feature which is not part of the response's HTTP headers. By default, if a route is cacheable then the middleware will try to return a cached response even if the query parameters were different.

For example if a request to `/foo/bar` has been cached, then the cached version will also be returned if a request is made to `/foo/bar?query1=a` or `/foo/bar?query1=a&query2=b`.

Sometimes this is not desired and the `VaryByQueryKeys` feature lets the [middleware vary its cached responses based on a request's query keys](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/middleware?#varybyquerykeys).

The generic `responseCaching` endpoint handler is the most basic response caching handler which can be used to configure custom response caching handlers as well as make use of the `VaryByQueryKeys` feature:

```fsharp
responseCaching
    (Some (CacheControlHeaderValue(MaxAge = TimeSpan.FromSeconds(30)))
    (Some "Accept, Accept-Encoding")
    (Some [| "query1"; "query2" |])
```

The first parameter is of type [CacheControlHeaderValue](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.headers.cachecontrolheadervalue).

The second parameter is an `string option` which defines the `vary` parameter.

The third and last parameter is a `string[] option` which defines an optional list of query parameter values which must be used to vary a cached response by the ASP.NET Core response caching middleware. Please be aware that this feature only applies to the ASP.NET Core response caching middleware and will not be respected by any intermediate proxy servers.

### Response Compression

ASP.NET Core has its own [Response Compression Middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression) which works out of the box with Oxpecker. There's no additional functionality or http handlers required in order to make it work with Oxpecker web applications.

## Testing

Integration testing of an Oxpecker application follows the concept of [ASP.NET Core testing](https://learn.microsoft.com/en-us/aspnet/core/test/middleware). You can check out the examples of tests in this repository itself: [Oxpecker.Tests](https://github.com/Lanayx/Oxpecker/tree/main/tests/Oxpecker.Tests)
