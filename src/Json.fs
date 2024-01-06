namespace Oxpecker

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
        abstract member Serialize<'T> : 'T * HttpContext -> Task
        abstract member Deserialize<'T> : HttpContext -> Task<'T>


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
    type Serializer (?options: JsonSerializerOptions) =
        let options = defaultArg options <| JsonSerializerOptions(JsonSerializerDefaults.Web)

        interface Json.ISerializer with
            member this.Serialize(dataObj, ctx) =
                ctx.Response.WriteAsJsonAsync(dataObj, options)

            member this.Deserialize(ctx) =
                ctx.Request.ReadFromJsonAsync(options).AsTask()