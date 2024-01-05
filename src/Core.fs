[<AutoOpen>]
module Oxpecker.Core

open System
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

// Fsharp-friendly alias for RequestDelegate
type EndpointHandler = HttpContext -> Task

// Fsharp-friendly alias for RequestDelegate
type EndpointMiddleware = EndpointHandler -> EndpointHandler

let inline compose_opImpl (_ : ^OpImpl) xs1 xs2 =
    ((^OpImpl or ^a) :(static member Compose: 'a * 'b -> 'b) (xs1, xs2))
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
/// Combines two <see cref="HttpHandler"/> functions into one.
/// Please mind that both <see cref="HttpHandler"/> functions will get pre-evaluated at runtime by applying the next <see cref="HttpFunc"/> parameter of each handler.
/// </summary>
let inline (>=>) xs1 xs2 = compose_opImpl Unchecked.defaultof<Composition> xs1 xs2


/// <summary>
/// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP Content-Length header accordingly, as well as the Content-Type header to text/plain.
/// </summary>
/// <param name="str">The string value to be send back to the client.</param>
/// <returns>A Oxpecker <see cref="HttpHandler" /> function which can be composed into a bigger web application.</returns>
let text (str : string) : EndpointHandler =
    let bytes = Encoding.UTF8.GetBytes str
    fun (ctx : HttpContext) ->
        ctx.SetContentType "text/plain; charset=utf-8"
        ctx.WriteBytesAsync bytes