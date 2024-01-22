namespace Oxpecker

open System.IO
open Microsoft.AspNetCore.Http
open System.Text
open System.Text.Json
open System.Threading.Tasks

[<RequireQualifiedAccess>]
module Json =

    /// <summary>
    /// Interface defining JSON serialization methods.
    /// Use this interface to customize JSON serialization in Oxpecker.
    /// </summary>
    [<AllowNullLiteral>]
    type ISerializer =
        abstract member Serialize<'T> : value: 'T * ctx: HttpContext * chunked: bool -> Task
        abstract member Deserialize<'T> : ctx: HttpContext -> Task<'T>


[<RequireQualifiedAccess>]
module SystemTextJson =

    /// <summary>
    /// <see cref="SystemTextJson.Serializer" /> is an alternaive <see cref="Json.ISerializer"/> in Oxpecker.
    ///
    /// It uses <see cref="System.Text.Json"/> as the underlying JSON serializer to (de-)serialize
    /// JSON content.
    /// For support of F# unions and records, look at https://github.com/Tarmil/FSharp.SystemTextJson
    /// which plugs into this serializer.
    /// </summary>
    type Serializer(?options: JsonSerializerOptions) =
        let options =
            defaultArg options <| JsonSerializerOptions(JsonSerializerDefaults.Web)

        interface Json.ISerializer with
            member this.Serialize(value, ctx, chunked) =
                if chunked then
                    ctx.Response.WriteAsJsonAsync(value, options)
                else
                    task {
                        use stream = recyclableMemoryStreamManager.Value.GetStream()
                        JsonSerializer.Serialize(stream, value, options)
                        ctx.Response.ContentType <- "application/json; charset=utf-8"
                        ctx.Response.Headers.ContentLength <- stream.Length
                        stream.Seek(0, SeekOrigin.Begin) |> ignore
                        return! stream.CopyToAsync(ctx.Response.Body)
                    }

            member this.Deserialize(ctx) =
                ctx.Request.ReadFromJsonAsync(options).AsTask()
