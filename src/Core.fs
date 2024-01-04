[<AutoOpen>]
module Oxpecker.Core

open System
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

// Fsharp-friendly alias for
type HttpHandler = HttpContext -> Task

let compose (handler1 : HttpHandler) (handler2 : HttpHandler) : HttpHandler =
    fun (ctx : HttpContext) ->
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

/// <summary>
/// Combines two <see cref="HttpHandler"/> functions into one.
/// Please mind that both <see cref="HttpHandler"/> functions will get pre-evaluated at runtime by applying the next <see cref="HttpFunc"/> parameter of each handler.
/// </summary>
let (>=>) = compose

/// <summary>
/// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP Content-Length header accordingly, as well as the Content-Type header to text/plain.
/// </summary>
/// <param name="str">The string value to be send back to the client.</param>
/// <returns>A Oxpecker <see cref="HttpHandler" /> function which can be composed into a bigger web application.</returns>
let text (str : string) : HttpHandler =
    let bytes = Encoding.UTF8.GetBytes str
    fun (ctx : HttpContext) ->
        ctx.SetContentType "text/plain; charset=utf-8"
        ctx.WriteBytesAsync bytes