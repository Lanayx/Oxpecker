[<AutoOpen>]
module Oxpecker.Core

open System
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

/// A type alias for <see cref="System.Threading.Tasks.Task{HttpContext option}" />  which represents the result of a HTTP function (HttpFunc).
/// If the result is Some HttpContext then the Oxpecker middleware will return the response to the client and end the pipeline. However, if the result is None then the Oxpecker middleware will continue the ASP.NET Core pipeline by invoking the next middleware.
/// </summary>
type HttpFuncResult = Task<HttpContext option>

/// <summary>
/// A HTTP function which takes an <see cref="Microsoft.AspNetCore.Http.HttpContext"/> object and returns a <see cref="HttpFuncResult"/>.
/// The function may inspect the incoming <see cref="Microsoft.AspNetCore.Http.HttpRequest"/> and make modifications to the <see cref="Microsoft.AspNetCore.Http.HttpResponse"/> before returning a <see cref="HttpFuncResult"/>. The result can be either a <see cref="System.Threading.Tasks.Task"/> of Some HttpContext or a <see cref="System.Threading.Tasks.Task"/> of None.
/// If the result is Some HttpContext then the Oxpecker middleware will return the response to the client and end the pipeline. However, if the result is None then the Oxpecker middleware will continue the ASP.NET Core pipeline by invoking the next middleware.
/// </summary>
type HttpFunc = HttpContext -> HttpFuncResult

/// <summary>
/// A HTTP handler is the core building block of a Oxpecker web application. It works similarly to ASP.NET Core's middleware where it is self responsible for invoking the next <see cref="HttpFunc"/> function of the pipeline or shortcircuit the execution by directly returning a <see cref="System.Threading.Tasks.Task"/> of HttpContext option.
/// </summary>
type HttpHandler = HttpFunc -> HttpFunc

/// <summary>
/// Use earlyReturn to shortcircuit the <see cref="HttpHandler"/> pipeline and return Some HttpContext to the surrounding <see cref="HttpHandler"/> or the Oxpecker middleware (which would subsequently end the pipeline by returning the response back to the client).
/// </summary>
let earlyReturn : HttpFunc = Some >> Task.FromResult

/// <summary>
/// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP Content-Length header accordingly, as well as the Content-Type header to text/plain.
/// </summary>
/// <param name="str">The string value to be send back to the client.</param>
/// <returns>A Oxpecker <see cref="HttpHandler" /> function which can be composed into a bigger web application.</returns>
let text (str : string) : HttpHandler =
    let bytes = Encoding.UTF8.GetBytes str
    fun (_ : HttpFunc) (ctx : HttpContext) ->
        ctx.SetContentType "text/plain; charset=utf-8"
        ctx.WriteBytesAsync bytes