# Oxpecker

[Nuget package](https://www.nuget.org/packages/Oxpecker)

Oxpecker is a functional wrapper around ASP.NET Core Endpoint routing (sometimes called Minimal API).

This library is basically a revised version of [Giraffe](https://github.com/giraffe-fsharp/Giraffe), it mostly sticks to it's successful API (hence the name). Improvements involve changing some core types, performance of template handlers, simplifying handlers and dropping a lot of outdated functionality.

Medium introductory post: https://medium.com/@lanayx/the-oxpecker-ef9df3dfb918

## Features:

- Native [ASP.NET Core Endpoint routing](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing) integration
- Strongly typed route parameters
- Endpoint handlers and endpoint middlewares, flexible composition
- JSON binding and serialization
- Form binding
- URL parameters binding
- Response caching
- Streaming
- Authorization
- eTag support
- Many useful **HttpContext** extensions

## Documentation:

TBD, for now you can use [Giraffe documentation](https://giraffe.wiki/docs), with the following differences:

- `routef` parameters should be surrounded with curly braces `{}`, this allows using [Route constraints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-8.0#route-constraints)
- `routef` handler arguments are now curried, not tuplified
- `HttpHandler` concept is separated into `EndpointHandler` and `EndpointMiddlware`. The difference is that the former doesn't accept `next` parameter, while the latter does.
- Case insensitive functions (`*Ci`) are dropped, since everything is case insensitive by default
- Some other route functions are dropped
- `JSON.ISerializer` only requires one method implemented
- Model binding will throw exceptions to be caught in common middleware (see [examples/Basic](https://github.com/Lanayx/Oxpecker/tree/main/examples/Basic))
- .NET 8 minimal target
- Oxpecker project is planned to be moved to `fsprojects` organization once it reaches 200 stars, expect high quality of maintenance
