namespace Oxpecker

open System.IO
open Microsoft.AspNetCore.Http
open System.Text.Json
open System.Threading.Tasks
open Microsoft.IO

/// <summary>
/// <para>Interface defining JSON serialization methods.
/// Use this interface to customize JSON serialization in Oxpecker.</para>
/// <para>Implementations must pass `ctx.RequestAborted` to every asynchronous read or write, so that serialization
/// stops when the client disconnects, and let the resulting `OperationCanceledException` propagate.</para>
/// </summary>
type IJsonSerializer =
    abstract member Serialize<'T> : value: 'T * ctx: HttpContext * chunked: bool -> Task
    abstract member Deserialize<'T> : ctx: HttpContext -> Task<'T>

/// <summary>
/// <see cref="Serializers.SystemTextJsonSerializer" /> is a default implementation of  <see cref="Serializers.IJsonSerializer"/> in Oxpecker.
///
/// It uses <see cref="System.Text.Json"/> as the underlying JSON serializer to (de-)serialize
/// JSON content.
/// For support of F# unions and records, look at https://github.com/Tarmil/FSharp.SystemTextJson
/// which plugs into this serializer.
/// </summary>
type SystemTextJsonSerializer(?options: JsonSerializerOptions) =
    let options =
        defaultArg options <| JsonSerializerOptions(JsonSerializerDefaults.Web)

    let serializeToStreamWithLength
        value
        (stream: RecyclableMemoryStream)
        (ctx: HttpContext)
        (options: JsonSerializerOptions)
        =
        ctx.RequestAborted.ThrowIfCancellationRequested()
        JsonSerializer.Serialize(stream, value, options)
        ctx.Response.ContentType <- "application/json; charset=utf-8"
        ctx.Response.Headers.ContentLength <- stream.Length
        stream.Seek(0, SeekOrigin.Begin) |> ignore
        if ctx.Request.Method <> HttpMethods.Head then
            stream.CopyToAsync(ctx.Response.Body, ctx.RequestAborted)
        else
            Task.CompletedTask

    interface IJsonSerializer with
        member this.Serialize(value, ctx, chunked) =
            if chunked then
                if ctx.Request.Method <> HttpMethods.Head then
                    ctx.Response.WriteAsJsonAsync(value, options, ctx.RequestAborted)
                else
                    ctx.Response.ContentType <- "application/json; charset=utf-8"
                    Task.CompletedTask
            else
                task {
                    use stream = recyclableMemoryStreamManager.Value.GetStream()
                    return! serializeToStreamWithLength value stream ctx options
                }

        member this.Deserialize(ctx) =
            task {
                match! ctx.Request.ReadFromJsonAsync(options, ctx.RequestAborted) with
                | null -> return Unchecked.defaultof<_>
                | v -> return v
            }
