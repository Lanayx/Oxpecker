---
render_with_liquid: false
---
# Oxpecker.OpenApi

`Oxpecker.OpenApi` extends `Oxpecker` framework with functionality to automatically generate OpenApi spec from code.

[Nuget package](https://www.nuget.org/packages/Oxpecker.OpenApi) `dotnet add package Oxpecker.OpenApi`

Two usages:

```fsharp
open Oxpecker
open Oxpecker.OpenApi

let endpoints = [
    // addOpenApi supports passing detailed configuration
    POST [
        route "/product" (text "Product posted!")
            |> addOpenApi (OpenApiConfig(
                requestBody = RequestBody(typeof<Product>),
                responseBodies = [| ResponseBody(typeof<string>) |],
                configureOperation = (fun o -> o.OperationId <- "PostProduct"; o)
            ))
    ]
    // addOpenApiSimple is a shortcut for simple cases
    GET [
        routef "/product/{%i}" (
            fun id ->
                products
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

Since `Oxpecker.OpenApi` works on top of `Microsoft.AspNetCore.OpenApi` and `Swashbuckle.AspNetCore` packages, you need to do [standard steps](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi):

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

To make endpoints discoverable by Swagger, you need to call one of the following functions: `addOpenApi` or `addOpenApiSimple` on the endpoint.

_NOTE: you don't have to describe routing parameters when using those functions, they will be inferred from the route template automatically._

### addOpenApi

This method is used to add OpenApi metadata to the endpoint. It accepts `OpenApiConfig` object with the following optional parameters:

```fsharp
type OpenApiConfig (?requestBody : RequestBody,
                    ?responseBodies : ResponseBody seq,
                    ?configureOperation : OpenApiOperation -> OpenApiOperation) =
    // ...
```
Response body schema will be inferred from the types passed to `requestBody` and `responseBodies` parameters. Each `ResponseBody` object in sequence must have different status code.
`configureOperation` parameter is a function that allows you to do very low-level modifications the `OpenApiOperation` object.

### addOpenApiSimple

This method is a shortcut for simple cases. It accepts two generic type parameters - request and response, so the schema can be inferred from them.

```fsharp
let addOpenApiSimple<'Req, 'Res> = ...
```
If your handler doesn't accept any input, you can pass `unit` as a request type (works for response as well).
