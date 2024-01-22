# Oxpecker

[Nuget package](https://www.nuget.org/packages/Oxpecker)

Examples can be found [here](https://github.com/Lanayx/Oxpecker/tree/develop/examples)

Performance tests reside [here](https://github.com/Lanayx/Oxpecker/tree/develop/tests/PerfTest)

## Documentation:

TBD, for now you can use [Giraffe documentation](https://giraffe.wiki/docs), with the following differences:

- `routef` parameters should be surrounded with curly braces `{}`, this allows using [Route constraints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-8.0#route-constraints)
- `routef` handler arguments are now curried, not tuplified
- `HttpHandler` concept is separated into `EndpointHandler` and `EndpointMiddlware`. The difference is that the former doesn't accept `next` parameter, while the latter does.
- Case insensitive functions (`*Ci`) are dropped, since everything is case insensitive by default
- Some other route functions are dropped
- `JSON.ISerializer` only requires one method implemented
- Model binding will throw exceptions to be caught in common middleware (see [examples/Basic](https://github.com/Lanayx/Oxpecker/tree/develop/examples/Basic))
- .NET 8 minimal target
- CE-based strongly typed ViewEngine built on class inheritance
