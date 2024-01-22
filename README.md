# Oxpecker

* [Oxpecker Nuget package](https://www.nuget.org/packages/Oxpecker)
* [Oxpecker.ViewEngine Nuget package](https://www.nuget.org/packages/Oxpecker.ViewEngine)

Oxpecker is a functional wrapper around ASP.NET Core Endpoint routing (sometimes called Minimal API).

This library is basically a revised version of [Giraffe](https://github.com/giraffe-fsharp/Giraffe), it mostly sticks to it's successful API (hence the name). Improvements involve changing some core types, performance of template handlers, simplifying handlers and dropping a lot of outdated functionality.

Medium introductory post: https://medium.com/@lanayx/the-oxpecker-ef9df3dfb918

Oxpecker project is planned to be moved to `fsprojects` organization once it reaches 200 stars

## Features:

- Native [ASP.NET Core Endpoint routing](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing) integration
- ViewEngine with convenient DSL for HTML
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

* [Oxpecker Readme](https://github.com/Lanayx/Oxpecker/blob/develop/src/Oxpecker/README.md)
* [Oxpecker.ViewEngine Readme](https://github.com/Lanayx/Oxpecker/blob/develop/src/Oxpecker/README.md)
