# Oxpecker

[Nuget package](https://www.nuget.org/packages/Oxpecker)

Examples can be found [here](https://github.com/Lanayx/Oxpecker/tree/develop/examples)

Performance tests reside [here](https://github.com/Lanayx/Oxpecker/tree/develop/tests/PerfTest)

## Documentation:

An in depth functional reference to all of Oxpecker's default features.

## Table of contents

- [Fundamentals](#fundamentals)
    - [Endpoint Routing](#endpointrouting)
    - [EndpointHandler](#endpointhandler)
    - [EndpointMiddleware](#endpointmiddleware)
    - [Oxpecker pipeline vs. ASP.NET Core pipeline](#oxpecker-pipeline-vs-aspnet-core-pipeline)
    - [Creating new EndpointHandler and EndpointMiddlware](#ways-of-creating-a-new-endpointhandler-and-endpointmiddleware)
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
    - [IResult Integration](#iresult-integration)
    - [Routing](#routing)
    - [Model Binding](#model-binding)

## Fundamentals

### EndpointRouting

Oxpecker is built on top of the ASP.NET Core [Endpoint Routing](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing) and provides some convenient DSL for F# users.

When using Oxpecker, make sure you are familiar with ASP.NET Core and it's concepts, since Oxpecker reuses a lot of built-in functionality.

### EndpointHandler

The main building block in Oxpecker is an `EndpointHandler`:

```fsharp
type EndpointHandler = HttpContext -> Task
```

an `EndpointHandler` is a function which takes `HttpContext`, and returns a `Task` when finished.

`EndpointHandler` function has full control of the incoming `HttpRequest` and the resulting `HttpResponse`. It closely follows [RequestDelegate](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.requestdelegate) signature, but in F# style.

`EndpointHandler` normally should be regarded as a _terminal_ handler, meaning that it should write some result in response (but not necessary, as described in composition section).

### EndpointMiddleware

```fsharp
type EndpointMiddleware = EndpointHandler -> HttpContext -> Task
```
`EndpointMiddleware` is similar to `EndpointHandler`, but accepts the _next_ `EndpointHandler` as first parameter.

Each `EndpointMiddleware` can process an incoming `HttpRequest` before passing it further down the Oxpecker pipeline by invoking the next `EndpointMiddleware` or short circuit the execution by returning the `Task` itself.

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
            ctx.TryGetQueryStringValue "name"
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
            let! person = ctx.BindJsonAsync<Person>()
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
#### Multi-route composition

Sometimes you want to compose some generic handler or middleware not only with one route, buth with the whole collection of routes. It is possible using `applyBefore` and `applyAfter` functions. For example:

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

let webApp = [
    route "/ping"   <| text "pong"
]

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseOxpecker(webApp) // Add Oxpecker to the ASP.NET Core pipeline, should go after UseRouting
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

The approach is described in the article https://bartoszsypytkowski.com/dealing-with-complex-dependency-injection-in-f/ , and to see how it looks in practice, you can refer to the [CRUD example in the repository.](https://github.com/Lanayx/Oxpecker/tree/develop/examples/CRUD)



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

Working with HTTP headers in Oxpecker is plain simple. The `TryGetRequestHeader (key: string)` extension method tries to retrieve the value of a given HTTP header and then returns either `Some string` or `None`:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        let someValue =
            match ctx.TryGetRequestHeader "X-MyOwnHeader" with
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

let webApp =
    [
        // Filters for GET requests
        GET  [
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

### IResult integration

If you only use JSON for communication and like what ASP.NET core IResult offers, you might be please to know that Oxpecker supports that as well. You can simplify returning responses together with status codes using `Microsoft.AspNetCore.Http.TypedResults`:

```fsharp
open Oxpecker
open type Microsoft.AspNetCore.Http.TypedResults

[<CLIMutable>]
type Person = {
    FirstName: string
    LastName: string
}

let johnDoe = {
    FirstName = "John"
    LastName  = "Doe"
}

let app = [
    route "/"     <| text "Hello World"
    route "/john" <| %Ok johnDoe // returns 200 OK with JSON body
    route "/bad"  <| %BadRequest()
]
```
The `%` operator is used to convert `IResult` to `EndpointHandler`. You can also do conversion inside EndpointHandler using `.Write` extension method:

```fsharp
let myHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.Write <| %Ok johnDoe
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

| Format Char | Type                            |
| ----------- |---------------------------------|
| `%b` | `bool`                          |
| `%c` | `char`                          |
| `%s` | `string`                        |
| `%i` | `int`                           |
| `%d` | `int64`                         |
| `%f` | `float`/`double`                |
| `%u` | `uint64`                        |
| `%O` | `Any object` (with [constraints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraints)) |

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

### Query Strings

Working with query strings is very similar to working with HTTP headers in Oxpecker. The `TryGetQueryStringValue (key : string)` extension method tries to retrieve the value of a given query string parameter and then returns either `Some string` or `None`:

```fsharp
let someHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        let someValue =
            match ctx.TryGetQueryStringValue "q" with
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
            return! ctx.Write <| Successful.OK car
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
        route "/car" (bindJson<Car> (fun car -> %Ok car))
    ]
]
```

Both, the `HttpContext` extension method as well as the `EndpointHandler` function will try to create an instance of type `'T` regardless if the submitted payload contained a complete representation of `'T` or not. The parsed object might only contain partial data (where some properties might be `null`) and additional `null` checks might be required before further processing.

Please note that in order for the model binding to work the record type must be decorated with the `[<CLIMutable>]` attribute, which will make sure that the type will have a parameterless constructor.

The underlying JSON serializer can be configured as a dependency during application startup (see [JSON](#json)).

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
