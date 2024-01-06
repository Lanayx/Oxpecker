[<AutoOpen>]
module Oxpecker.Core

open System.Threading.Tasks
open Microsoft.AspNetCore.Http

/// Fsharp-friendly alias for RequestDelegate
type EndpointHandler = HttpContext -> Task

/// Endpoint middleware is analogue to ASP.NET Core Middleware, but for Endpoint level.
type EndpointMiddleware = EndpointHandler -> EndpointHandler

let inline compose_opImpl (_ : ^OpImpl) left right =
    ((^OpImpl or ^left) :(static member Compose: ^left * ^right -> ^right) (left, right))
type Composition =
    static member Compose (handler1: EndpointHandler, handler2: EndpointHandler): EndpointHandler =
        fun (ctx: HttpContext) ->
            match ctx.Response.HasStarted with
            | true  ->
                Task.CompletedTask
            | false ->
                task {
                    do! handler1 ctx
                    match ctx.Response.HasStarted with
                    | true -> ()
                    | false ->
                        return! handler2 ctx
                }

    static member Compose (middleware: EndpointMiddleware, handler: EndpointHandler): EndpointHandler =
        fun (ctx: HttpContext) ->
            match ctx.Response.HasStarted with
            | true  ->
                Task.CompletedTask
            | false ->
                middleware handler ctx

    static member Compose (middleware1: EndpointMiddleware, middleware2: EndpointMiddleware): EndpointMiddleware =
        fun (next: EndpointHandler) ->
            let resultMiddleware = next |> middleware1 |> middleware2
            fun (ctx: HttpContext) ->
                match ctx.Response.HasStarted with
                | true  -> next ctx
                | false -> resultMiddleware ctx

/// <summary>
/// Combines two <see cref="EndpointHandler"/> or two <see cref="EndpointMiddleware"/> functions into one. Also can combine middleware with handler (but not vise versa)
/// </summary>
let inline (>=>) left right = compose_opImpl Unchecked.defaultof<Composition> left right

/// <summary>
/// Parses a JSON payload into an instance of type 'T.
/// </summary>
/// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
/// <param name="ctx">HttpContext</param>
/// <typeparam name="'T"></typeparam>
/// <returns>A Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
let bindJson<'T> (f: 'T -> EndpointHandler) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let! model = ctx.BindJson<'T>()
            return! f model ctx
        }


/// <summary>
/// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP Content-Length header accordingly, as well as the Content-Type header to text/plain.
/// </summary>
/// <param name="str">The string value to be send back to the client.</param>
/// <param name="ctx">HttpContext</param>
/// <returns>A Oxpecker <see cref="EndpointHandler" /> function which can be composed into a bigger web application.</returns>
let text (str : string) : EndpointHandler =
    fun (ctx : HttpContext) ->
        ctx.WriteText str

/// <summary>
/// Serializes an object to JSON and writes the output to the body of the HTTP response.
/// It also sets the HTTP Content-Type header to application/json and sets the Content-Length header accordingly.
/// The JSON serializer can be configured in the ASP.NET Core startup code by registering a custom class of type <see cref="Json.ISerializer"/>.
/// </summary>
/// <param name="dataObj">The object to be send back to the client.</param>
/// <param name="ctx"></param>
/// <typeparam name="'T"></typeparam>
/// <returns>A Oxpecker <see cref="EndpointHandler" /> function which can be composed into a bigger web application.</returns>
let json<'T> (dataObj: 'T) : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson dataObj