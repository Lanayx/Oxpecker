namespace Oxpecker

open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

[<AutoOpen>]
module CoreTypes =

    /// Fsharp-friendly alias for RequestDelegate
    type EndpointHandler = HttpContext -> Task

    /// Endpoint middleware is analogue to ASP.NET Core Middleware, but for Endpoint level.
    type EndpointMiddleware = EndpointHandler -> EndpointHandler

module CoreInternal =

    let inline compose_opImpl (_: ^OpImpl) left right =
        ((^OpImpl or ^left): (static member Compose: ^left * ^right -> ^right) (left, right))
    type Composition =
        static member Compose(handler1: EndpointHandler, handler2: EndpointHandler) : EndpointHandler =
            fun (ctx: HttpContext) ->
                match ctx.Response.HasStarted with
                | true -> Task.CompletedTask
                | false ->
                    task {
                        do! handler1 ctx
                        match ctx.Response.HasStarted with
                        | true -> ()
                        | false -> return! handler2 ctx
                    }

        static member Compose(middleware: EndpointMiddleware, handler: EndpointHandler) : EndpointHandler =
            fun (ctx: HttpContext) ->
                match ctx.Response.HasStarted with
                | true -> Task.CompletedTask
                | false -> middleware handler ctx

        static member Compose(middleware1: EndpointMiddleware, middleware2: EndpointMiddleware) : EndpointMiddleware =
            fun (next: EndpointHandler) ->
                let resultMiddleware = next |> middleware2 |> middleware1
                fun (ctx: HttpContext) ->
                    match ctx.Response.HasStarted with
                    | true -> next ctx
                    | false -> resultMiddleware ctx


[<AutoOpen>]
module Core =

    open CoreInternal

    /// <summary>
    /// Combines two <see cref="EndpointHandler"/> or two <see cref="EndpointMiddleware"/> functions into one. Also can combine middleware with handler (but not vise versa)
    /// </summary>
    let inline (>=>) left right =
        compose_opImpl Unchecked.defaultof<Composition> left right

    /// Same as >=>, but with additional argument
    let inline (>>=>) left right x =
        compose_opImpl Unchecked.defaultof<Composition> left (right x)

    /// Same as >=>, but with two arguments
    let inline (>>=>+) left right x y =
        compose_opImpl Unchecked.defaultof<Composition> left (right x y)

    /// Same as >=>, but with three arguments
    let inline (>>=>++) left right x y z =
        compose_opImpl Unchecked.defaultof<Composition> left (right x y z)
