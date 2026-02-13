namespace Oxpecker

open System.IO
open Microsoft.AspNetCore.Http
open System.Text.Json
open System.Threading.Tasks
open Microsoft.IO

/// <summary>
/// Interface defining JSON serialization methods.
/// Use this interface to customize JSON serialization in Oxpecker.
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
        JsonSerializer.Serialize(stream, value, options)
        ctx.Response.ContentType <- "application/json; charset=utf-8"
        ctx.Response.Headers.ContentLength <- stream.Length
        stream.Seek(0, SeekOrigin.Begin) |> ignore
        if ctx.Request.Method <> HttpMethods.Head then
            stream.CopyToAsync(ctx.Response.Body)
        else
            Task.CompletedTask

    interface IJsonSerializer with
        member this.Serialize(value, ctx, chunked) =
            if chunked then
                ctx.Response.ContentType <- "application/json; charset=utf-8"
                if ctx.Request.Method <> HttpMethods.Head then
                    // Use JsonSerializer.SerializeAsync directly with our configured options instead of
                    // ctx.Response.WriteAsJsonAsync to ensure consistent behavior between chunked and
                    // non-chunked serialization. WriteAsJsonAsync delegates to ASP.NET Core's JSON pipeline
                    // (IOptions<JsonOptions>) which may use different settings than Oxpecker's configured
                    // JsonSerializerOptions. By using JsonSerializer.SerializeAsync directly, both paths
                    // honor the same options instance, providing predictable and consistent serialization.
                    JsonSerializer.SerializeAsync(ctx.Response.Body, value, options)
                else
                    // For HEAD requests, we still want to set the Content-Type header and return an empty body
                    task {
                        use stream = recyclableMemoryStreamManager.Value.GetStream()
                        return! serializeToStreamWithLength value stream ctx options
                    }
            else
                task {
                    use stream = recyclableMemoryStreamManager.Value.GetStream()
                    return! serializeToStreamWithLength value stream ctx options
                }

        member this.Deserialize(ctx) =
            task {
                match! ctx.Request.ReadFromJsonAsync(options) with
                | null -> return Unchecked.defaultof<_>
                | v -> return v
            }
