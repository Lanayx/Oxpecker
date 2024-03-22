# Oxpecker.OpenApi

`Oxpecker.OpenApi` extends `Oxpecker` framework with functionality to automatically generate OpenApi spec from code.

[Nuget package](https://www.nuget.org/packages/Oxpecker.OpenApi)

Two usages:

```fsharp
open Oxpecker
open Oxpecker.OpenApi

let endpoints = [
    // addOpenApi supports passing detailed configuration
    POST [
        route "/product" (text "Product posted!")
        |> addOpenApi (OpenApiConfig(
            requestInfo = RequestInfo(typeof<Product>),
            responseInfos = [| ResponseInfo(typeof<string>) |],
            configureOperation = (fun o -> o.OperationId <- "PostProduct"; o)
        ))
    ]
    // addOpenApiSimple is a shortcut for simple cases
    GET [
        routef "/product/{%i}" (fun id ->
            forecases
            |> Array.find (fun f -> f.Id = num)
            |> json
        )
        |> configureEndpoint _.WithName("GetProduct")
        |> addOpenApiSimple<int, Product>
    ]
]

```

## Documentation:

### Integration

Since `Oxpecker.OpenApi` works on top of `Microsoft.AspNetCore.OpenApi` package, you need to do [standard steps](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi):

```fsharp

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .Use(errorHandler)
        .UseOxpecker(endpoints)
        .UseSwagger() // for generating OpenApi spec
        .UseSwaggerUI() // for viewing Swagger UI
        .Run(notFoundHandler)

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddOxpecker()
        .AddEndpointsApiExplorer() // use the API Explorer to discover and describe endpoints
        .AddSwaggerGen() // swagger dependencies
    |> ignore
```

### addOpenApi

This method is used to add OpenApi metadata to the endpoint. It accepts `OpenApiConfig` object with the following properties:

```fsharp
type OpenApiConfig (?requestInfo : RequestInfo,
                    ?responseInfos : ResponseInfo seq,
                    ?configureOperation : OpenApiOperation -> OpenApiOperation) =
    // ...
```
Schema will be inferred from the types passed to `RequestInfo` and `ResponseInfos` objects. Each `ResponseInfo` object must have different status code.


### addOpenApiSimple

This method is a shortcut for simple cases. It accepts two types: request and response. Schema will be inferred from the types passed to the method.

```fsharp
let addOpenApiSimple<'Req, 'Res> = ...
```
If your handler doesn't accept any input, you can pass `unit` as a request type (works for response as well).
