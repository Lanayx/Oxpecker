﻿[<AutoOpen>]
module Oxpecker.Preconditional

open System
open System.Linq
open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Headers
open Microsoft.Extensions.Primitives
open Microsoft.Net.Http.Headers

let inline private cutOffMs (dt: DateTimeOffset) =
    DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Offset)

[<Struct>]
type Precondition =
    | NoConditionsSpecified
    | ResourceNotModified
    | ConditionFailed
    | AllConditionsMet

[<AutoOpen>]
module EntityTagHelper =
    /// <summary>
    /// Creates an object of type <see cref="EntityTagHeaderValue"/>.
    /// </summary>
    /// <param name="isWeak">The difference between a regular (strong) ETag and a weak ETag is that a matching strong ETag guarantees the file is byte-for-byte identical, whereas a matching weak ETag indicates that the content is semantically the same. So if the content of the file changes, the weak ETag should change as well.</param>
    /// <param name="eTag">The entity tag value (without quotes or the W/ prefix).</param>
    /// <returns>Returns an object of <see cref="EntityTagHeaderValue"/>.</returns>
    let private fromString (isWeak: bool) (eTag: string) =
        let eTagValue = $"\"%s{eTag}\""
        EntityTagHeaderValue(StringSegment(eTagValue), isWeak)

    let createETag (eTag: string) = fromString false eTag

    let createWeakETag (eTag: string) = fromString true eTag

type HttpContext with

    member private this.IsHeadOrGetRequest() =
        HttpMethods.IsHead this.Request.Method || HttpMethods.IsGet this.Request.Method

    member private this.ValidateIfMatch (eTag: EntityTagHeaderValue option) (requestHeaders: RequestHeaders) =
        match requestHeaders.IfMatch.Any() with
        | false -> NoConditionsSpecified
        | true ->
            match eTag with
            | None -> ConditionFailed
            | Some eTag ->
                requestHeaders.IfMatch
                |> Seq.exists _.Compare(eTag, true)
                |> function
                    | true -> AllConditionsMet
                    | false -> ConditionFailed

    member private this.ValidateIfUnmodifiedSince
        (lastModified: DateTimeOffset option)
        (requestHeaders: RequestHeaders)
        =
        match requestHeaders.IfUnmodifiedSince.HasValue with
        | false -> NoConditionsSpecified
        | true ->
            match lastModified with
            | None -> AllConditionsMet
            | Some lastModified ->
                let lastModified = cutOffMs lastModified
                match
                    requestHeaders.IfUnmodifiedSince.Value > (cutOffMs DateTimeOffset.UtcNow)
                    || requestHeaders.IfUnmodifiedSince.Value >= lastModified
                with
                | true -> AllConditionsMet
                | false -> ConditionFailed

    member private this.ValidateIfNoneMatch (eTag: EntityTagHeaderValue option) (requestHeaders: RequestHeaders) =
        match requestHeaders.IfNoneMatch.Any() with
        | false -> NoConditionsSpecified
        | true ->
            match eTag with
            | None -> AllConditionsMet
            | Some eTag ->
                requestHeaders.IfNoneMatch
                |> Seq.exists _.Compare(eTag, false)
                |> function
                    | false -> AllConditionsMet
                    | true ->
                        match this.IsHeadOrGetRequest() with
                        | true -> ResourceNotModified
                        | false -> ConditionFailed

    member private this.ValidateIfModifiedSince (lastModified: DateTimeOffset option) (requestHeaders: RequestHeaders) =
        match requestHeaders.IfModifiedSince.HasValue && this.IsHeadOrGetRequest() with
        | false -> NoConditionsSpecified
        | true ->
            match lastModified with
            | None -> AllConditionsMet
            | Some lastModified ->
                let lastModified = cutOffMs lastModified
                match
                    requestHeaders.IfModifiedSince.Value <= (cutOffMs DateTimeOffset.UtcNow)
                    && requestHeaders.IfModifiedSince.Value < lastModified
                with
                | true -> AllConditionsMet
                | false -> ResourceNotModified

type PreconditionExtensions() =
    /// <summary>
    /// Validates the following conditional HTTP headers of the HTTP request:
    ///
    /// If-Match
    ///
    /// If-None-Match
    ///
    /// If-Modified-Since
    ///
    /// If-Unmodified-Since
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="eTag">Optional ETag. You can use the static EntityTagHeaderValue.FromString helper method to generate a valid <see cref="EntityTagHeaderValue"/> object.</param>
    /// <param name="lastModified">Optional <see cref="System.DateTimeOffset"/> object denoting the last modified date.</param>
    /// <returns>
    /// Returns a Precondition union type, which can have one of the following cases:
    ///
    /// NoConditionsSpecified: No validation has taken place, because the client didn't send any conditional HTTP headers.
    ///
    /// ConditionFailed: At least one condition couldn't be satisfied. It is advised to return a 412 status code back to the client (you can use the HttpContext.PreconditionFailedResponse() method for that purpose).
    ///
    /// ResourceNotModified: The resource hasn't changed since the last visit. The server can skip processing this request and return a 304 status code back to the client (you can use the HttpContext.NotModifiedResponse() method for that purpose).
    ///
    /// AllConditionsMet: All pre-conditions can be satisfied. The server should continue processing the request as normal.
    /// </returns>
    [<Extension>]
    static member ValidatePreconditions
        (ctx: HttpContext, eTag: EntityTagHeaderValue option, lastModified: DateTimeOffset option)
        =
        // Parse headers
        let responseHeaders = ctx.Response.GetTypedHeaders()
        let requestHeaders = ctx.Request.GetTypedHeaders()

        // Helper bind functions to chain validation functions
        let bind (result: RequestHeaders -> Precondition) =
            function
            | NoConditionsSpecified -> result requestHeaders
            | AllConditionsMet ->
                match result requestHeaders with
                | NoConditionsSpecified -> AllConditionsMet
                | AllConditionsMet -> AllConditionsMet
                | ConditionFailed -> ConditionFailed
                | ResourceNotModified -> ResourceNotModified
            | ConditionFailed -> ConditionFailed
            | ResourceNotModified -> ResourceNotModified

        let ifNotSpecified (result: RequestHeaders -> Precondition) =
            function
            | NoConditionsSpecified -> result requestHeaders
            | AllConditionsMet -> AllConditionsMet
            | ConditionFailed -> ConditionFailed
            | ResourceNotModified -> ResourceNotModified

        // Set ETag and Last-Modified in the response
        if eTag.IsSome then
            responseHeaders.ETag <- eTag.Value
        if lastModified.IsSome then
            responseHeaders.LastModified <- cutOffMs lastModified.Value

        // Validate headers in correct precedence
        // RFC: https://tools.ietf.org/html/rfc7232#section-6
        requestHeaders
        |> ctx.ValidateIfMatch eTag
        |> ifNotSpecified(ctx.ValidateIfUnmodifiedSince lastModified)
        |> bind(ctx.ValidateIfNoneMatch eTag)
        |> ifNotSpecified(ctx.ValidateIfModifiedSince lastModified)

    /// <summary>
    /// Sends a default HTTP 304 Not Modified response to the client.
    /// </summary>
    /// <returns></returns>
    [<Extension>]
    static member NotModifiedResponse(ctx: HttpContext) =
        ctx.SetStatusCode StatusCodes.Status304NotModified

    /// <summary>
    /// Sends a default HTTP 412 Precondition Failed response to the client.
    /// </summary>
    /// <returns></returns>
    [<Extension>]
    static member PreconditionFailedResponse(ctx: HttpContext) =
        ctx.SetStatusCode StatusCodes.Status412PreconditionFailed

// ---------------------------
// EndpointHandler functions
// ---------------------------

/// <summary>
/// Validates the following conditional HTTP headers of the request:
///
/// If-Match
///
/// If-None-Match
///
/// If-Modified-Since
///
/// If-Unmodified-Since
///
///
/// If the conditions are met (or non existent) then it will invoke the next http handler in the pipeline otherwise it will return a 304 Not Modified or 412 Precondition Failed response.
/// </summary>
/// <param name="eTag">Optional ETag. You can use the static EntityTagHeaderValue.FromString helper method to generate a valid <see cref="EntityTagHeaderValue"/> object.</param>
/// <param name="lastModified">Optional <see cref="System.DateTimeOffset"/> object denoting the last modified date.</param>
/// <param name="next"></param>
/// <param name="ctx"></param>
/// <returns>An Oxpecker <see cref="HttpHandler" /> function which can be composed into a bigger web application.</returns>
let validatePreconditions
    (eTag: EntityTagHeaderValue option)
    (lastModified: DateTimeOffset option)
    : EndpointMiddleware =
    fun (next: EndpointHandler) (ctx: HttpContext) ->
        task {
            match ctx.ValidatePreconditions(eTag, lastModified) with
            | ConditionFailed -> return ctx.PreconditionFailedResponse()
            | ResourceNotModified -> return ctx.NotModifiedResponse()
            | AllConditionsMet
            | NoConditionsSpecified -> return! next ctx
        }
