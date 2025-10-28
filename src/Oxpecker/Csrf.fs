namespace Oxpecker

[<AutoOpen>]
module Csrf =
    open System.Threading.Tasks
    open Microsoft.AspNetCore.Antiforgery
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.Logging

    /// <summary>
    /// Default CSRF token header name used by ASP.NET Core antiforgery system
    /// </summary>
    [<Literal>]
    let DefaultCsrfTokenHeaderName = "X-CSRF-TOKEN"

    /// <summary>
    /// Default CSRF token form field name used by ASP.NET Core antiforgery system
    /// </summary>
    [<Literal>]
    let DefaultCsrfTokenFormFieldName = "__RequestVerificationToken"

    /// <summary>
    /// Validates the CSRF token from the incoming request.
    /// This middleware checks tokens from both HTTP headers (X-CSRF-TOKEN) and form fields (__RequestVerificationToken).
    /// Returns 403 Forbidden if validation fails.
    /// </summary>
    /// <param name="authFailedHandler">Handler to execute when CSRF validation fails. If not provided, returns 403.</param>
    /// <param name="next">Next handler in the pipeline</param>
    /// <param name="ctx">HTTP context</param>
    /// <returns>An Oxpecker <see cref="EndpointMiddleware"/> function which can be composed into a bigger web application.</returns>
    let validateCsrfToken (authFailedHandler: EndpointHandler option) : EndpointMiddleware =
        fun (next: EndpointHandler) (ctx: HttpContext) ->
            task {
                let antiforgery = ctx.GetService<IAntiforgery>()
                try
                    let! isValid = antiforgery.IsRequestValidAsync(ctx)
                    if isValid then
                        return! next ctx
                    else
                        let logger = ctx.GetLogger("Oxpecker.Csrf")
                        logger.LogWarning(
                            "CSRF token validation failed for {Method} {Path}",
                            ctx.Request.Method,
                            ctx.Request.Path
                        )
                        match authFailedHandler with
                        | Some handler -> return! handler ctx
                        | None ->
                            ctx.SetStatusCode StatusCodes.Status403Forbidden
                            return! ctx.WriteText "CSRF token validation failed"
                with ex ->
                    let logger = ctx.GetLogger("Oxpecker.Csrf")
                    logger.LogWarning(
                        ex,
                        "CSRF token validation error for {Method} {Path}",
                        ctx.Request.Method,
                        ctx.Request.Path
                    )
                    match authFailedHandler with
                    | Some handler -> return! handler ctx
                    | None ->
                        ctx.SetStatusCode StatusCodes.Status403Forbidden
                        return! ctx.WriteText "CSRF token validation failed"
            }

    /// <summary>
    /// Validates the CSRF token using default 403 response on failure.
    /// Alias for validateCsrfToken with no custom failure handler.
    /// </summary>
    let requireAntiforgeryToken: EndpointMiddleware = validateCsrfToken None

    /// <summary>
    /// Generates CSRF tokens and stores them in HttpContext.Items for use in views.
    /// The tokens can be accessed via ctx.Items["CsrfToken"] (request token) and ctx.Items["CsrfTokenHeaderName"].
    /// Also sets the antiforgery cookie in the response.
    /// </summary>
    /// <param name="next">Next handler in the pipeline</param>
    /// <param name="ctx">HTTP context</param>
    /// <returns>An Oxpecker <see cref="EndpointMiddleware"/> function which can be composed into a bigger web application.</returns>
    let generateCsrfToken: EndpointMiddleware =
        fun (next: EndpointHandler) (ctx: HttpContext) ->
            task {
                let antiforgery = ctx.GetService<IAntiforgery>()
                let tokens = antiforgery.GetAndStoreTokens(ctx)

                ctx.Items["CsrfToken"] <- tokens.RequestToken
                ctx.Items["CsrfTokenHeaderName"] <- tokens.HeaderName

                return! next ctx
            }

    /// <summary>
    /// Returns CSRF token information as JSON for AJAX/API requests.
    /// Response format: { "token": "...", "headerName": "X-CSRF-TOKEN" }
    /// Sets antiforgery cookie if not already present.
    /// </summary>
    /// <param name="ctx">HTTP context</param>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let csrfTokenJson: EndpointHandler =
        fun (ctx: HttpContext) ->
            task {
                let antiforgery = ctx.GetService<IAntiforgery>()
                let tokens = antiforgery.GetAndStoreTokens(ctx)

                let response = {|
                    token = tokens.RequestToken
                    headerName = tokens.HeaderName
                |}

                return! ctx.WriteJson response
            }

    /// <summary>
    /// Returns CSRF token as an HTML hidden input field suitable for forms.
    /// Output format: &lt;input type="hidden" name="X-CSRF-TOKEN" value="..." /&gt;
    /// Sets antiforgery cookie if not already present.
    /// </summary>
    /// <param name="ctx">HTTP context</param>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let csrfTokenHtml: EndpointHandler =
        fun (ctx: HttpContext) ->
            task {
                let antiforgery = ctx.GetService<IAntiforgery>()
                let tokens = antiforgery.GetAndStoreTokens(ctx)

                let html =
                    sprintf "<input type=\"hidden\" name=\"%s\" value=\"%s\" />" tokens.HeaderName tokens.RequestToken

                return! ctx.WriteHtmlString html
            }

    /// <summary>
    /// Returns CSRF token string directly.
    /// Useful when you need to embed the token in custom HTML or JavaScript.
    /// Sets antiforgery cookie if not already present.
    /// </summary>
    /// <param name="ctx">HTTP context</param>
    /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
    let csrfToken: EndpointHandler =
        fun (ctx: HttpContext) ->
            task {
                let antiforgery = ctx.GetService<IAntiforgery>()
                let tokens = antiforgery.GetAndStoreTokens(ctx)
                return! ctx.WriteText tokens.RequestToken
            }
