// built-in endpoint handlers
namespace Oxpecker

open System.Globalization
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine

[<AutoOpen>]
module RequestHandlers =
    /// <summary>
    /// Parses a JSON payload into an instance of type 'T.
    /// </summary>
    /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
    /// <param name="ctx">HttpContext</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let bindJson<'T> (f: 'T -> EndpointHandler) : EndpointHandler =
        fun (ctx: HttpContext) ->
            task {
                let! model = ctx.BindJson<'T>()
                return! f model ctx
            }

    /// <summary>
    /// Parses a HTTP form payload into an instance of type 'T.
    /// </summary>
    /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
    /// <param name="ctx">HttpContext</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let bindForm<'T> (f: 'T -> EndpointHandler) : EndpointHandler =
        fun (ctx: HttpContext) ->
            task {
                let! model = ctx.BindForm<'T>()
                return! f model ctx
            }

    /// <summary>
    /// Parses a HTTP form payload into an instance of type 'T.
    /// </summary>
    /// <param name="culture"><see cref="System.Globalization.CultureInfo"/> to be used when parsing culture specific data such as float, DateTime or decimal values.</param>
    /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
    /// <param name="ctx">HttpContext</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let bindFormC<'T> (culture: CultureInfo) (f: 'T -> EndpointHandler) : EndpointHandler =
        fun (ctx: HttpContext) ->
            task {
                let! model = ctx.BindForm<'T> culture
                return! f model ctx
            }

    /// <summary>
    /// Parses a HTTP query string into an instance of type 'T.
    /// </summary>
    /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
    /// <param name="ctx">HttpContext</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let bindQuery<'T> (f: 'T -> EndpointHandler) : EndpointHandler =
        fun (ctx: HttpContext) ->
            let model = ctx.BindQuery<'T>()
            f model ctx

    /// <summary>
    /// Parses a HTTP query string into an instance of type 'T.
    /// </summary>
    /// <param name="culture"><see cref="System.Globalization.CultureInfo"/> to be used when parsing culture specific data such as float, DateTime or decimal values.</param>
    /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
    /// <param name="ctx">HttpContext</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let bindQueryC<'T> (culture: CultureInfo) (f: 'T -> EndpointHandler) : EndpointHandler =
        fun (ctx: HttpContext) ->
            let model = ctx.BindQuery<'T> culture
            f model ctx

[<AutoOpen>]
module ResponseHandlers =

    /// <summary>
    /// Redirects to a different location with a `302` or `301` (when permanent) HTTP status code.
    /// </summary>
    /// <param name="location">The URL to redirect the client to.</param>
    /// <param name="permanent">If true the redirect is permanent (301), otherwise temporary (302).</param>
    /// <param name="ctx">HttpContext</param>
    /// <returns>An Oxpecker <see cref="EndpointHandler" /> function which can be composed into a bigger web application.</returns>
    let redirectTo (location: string) (permanent: bool) : EndpointHandler =
        fun (ctx: HttpContext) ->
            ctx.Response.Redirect(location, permanent)
            Task.CompletedTask

    /// <summary>
    /// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP Content-Length header accordingly, as well as the Content-Type header to text/plain.
    /// </summary>
    /// <param name="str">The string value to be send back to the client.</param>
    /// <param name="ctx">HttpContext</param>
    /// <returns>An Oxpecker <see cref="EndpointHandler" /> function which can be composed into a bigger web application.</returns>
    let text (str: string) : EndpointHandler =
        fun (ctx: HttpContext) -> ctx.WriteText str

    /// <summary>
    /// Serializes an object to JSON and writes the output to the body of the HTTP response.
    /// It also sets the HTTP Content-Type header to application/json and sets the Content-Length header accordingly.
    /// The JSON serializer can be configured in the ASP.NET Core startup code by registering a custom class of type <see cref="Json.ISerializer"/>.
    /// </summary>
    /// <param name="value">The object to be send back to the client.</param>
    /// <param name="ctx">HttpContext</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>An Oxpecker <see cref="EndpointHandler" /> function which can be composed into a bigger web application.</returns>
    let json<'T> (value: 'T) : EndpointHandler =
        fun (ctx: HttpContext) -> ctx.WriteJson(value)

    /// <summary>
    /// Serializes an object to JSON and writes the output to the body of the HTTP response using chunked transfer encoding.
    /// It also sets the HTTP Content-Type header to application/json and sets the Transfer-Encoding header to chunked.
    /// The JSON serializer can be configured in the ASP.NET Core startup code by registering a custom class of type <see cref="Json.ISerializer"/>.
    /// </summary>
    /// <param name="value">The object to be send back to the client.</param>
    /// <param name="ctx"></param>
    /// <returns>An Oxpecker <see cref="EndpointHandler" /> function which can be composed into a bigger web application.</returns>
    let jsonChunked<'T> (value: 'T) : EndpointHandler =
        fun (ctx: HttpContext) -> ctx.WriteJsonChunked(value)

    /// <summary>
    /// Writes a HTML string to the body of the HTTP response.
    /// It also sets the HTTP header Content-Type to text/html and sets the Content-Length header accordingly.
    /// </summary>
    /// <param name="html">The HTML string to be send back to the client.</param>
    /// <param name="ctx"></param>
    /// <returns>A Oxpecker <see cref="EndpointHandler" /> function which can be composed into a bigger web application.</returns>
    let htmlString (html: string) : EndpointHandler =
        fun (ctx: HttpContext) -> ctx.WriteHtmlString html

    /// <summary>
    /// <para>Compiles a `Oxpecker.OxpeckerViewEngine.Builder.HtmlElement` object to a HTML view and writes the output to the body of the HTTP response.</para>
    /// <para>It also sets the HTTP header `Content-Type` to `text/html` and sets the `Content-Length` header accordingly.</para>
    /// </summary>
    /// <param name="htmlView">An `HtmlElement` object to be send back to the client and which represents a valid HTML view.</param>
    /// <param name="ctx"></param>
    /// <returns>A Oxpecker <see cref="EndpointHandler" /> function which can be composed into a bigger web application.</returns>
    let htmlView (htmlView: HtmlElement) : EndpointHandler =
        fun (ctx: HttpContext) -> ctx.WriteHtmlView htmlView

    /// <summary>
    /// Clears the current <see cref="Microsoft.AspNetCore.Http.HttpResponse"/> object.
    /// This can be useful if a <see cref="HttpHandler"/> function needs to overwrite the response of all previous <see cref="HttpHandler"/> functions with its own response (most commonly used by an <see cref="ErrorHandler"/> function).
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns>A Oxpecker <see cref="HttpHandler"/> function which can be composed into a bigger web application.</returns>
    let clearResponse: EndpointHandler =
        fun (ctx: HttpContext) ->
            ctx.Response.Clear()
            Task.CompletedTask

    /// <summary>
    /// Sets the Content-Type HTTP header in the response.
    /// </summary>
    /// <param name="contentType">The mime type of the response (e.g.: application/json or text/html).</param>
    /// <param name="ctx"></param>
    /// <returns>A Oxpecker <see cref="HttpHandler"/> function which can be composed into a bigger web application.</returns>
    let setContentType (contentType: string) : EndpointHandler =
        fun (ctx: HttpContext) ->
            ctx.SetContentType contentType
            Task.CompletedTask

    /// <summary>
    /// Sets the HTTP status code of the response.
    /// </summary>
    /// <param name="statusCode">The status code to be set in the response. For convenience you can use the static <see cref="Microsoft.AspNetCore.Http.StatusCodes"/> class for passing in named status codes instead of using pure int values.</param>
    /// <param name="ctx"></param>
    /// <returns>A Oxpecker <see cref="HttpHandler"/> function which can be composed into a bigger web application.</returns>
    let setStatusCode (statusCode: int) : EndpointHandler =
        fun (ctx: HttpContext) ->
            ctx.SetStatusCode statusCode
            Task.CompletedTask

    ///   <summary>
    /// Adds or sets a HTTP header in the response.
    /// </summary>
    /// <param name="key">The HTTP header name. For convenience you can use the static <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> class for passing in strongly typed header names instead of using pure string values.</param>
    /// <param name="value">The value to be set. Non string values will be converted to a string using the object's ToString() method.</param>
    /// <param name="ctx"></param>
    /// <returns>A Oxpecker <see cref="HttpHandler"/> function which can be composed into a bigger web application.</returns>
    let setHttpHeader (key: string) (value: string) : EndpointHandler =
        fun (ctx: HttpContext) ->
            ctx.SetHttpHeader(key, value)
            Task.CompletedTask
