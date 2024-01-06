namespace Oxpecker

[<RequireQualifiedAccess>]
module Json =
    open System.IO
    open System.Threading.Tasks

    /// <summary>
    /// Interface defining JSON serialization methods.
    /// Use this interface to customize JSON serialization in Oxpecker.
    /// </summary>
    [<AllowNullLiteral>]
    type ISerializer =
        abstract member SerializeToString<'T>      : 'T -> string
        abstract member SerializeToBytes<'T>       : 'T -> byte array
        abstract member SerializeToStreamAsync<'T> : 'T -> Stream -> Task

        abstract member Deserialize<'T>            : string -> 'T
        abstract member Deserialize<'T>            : byte[] -> 'T
        abstract member DeserializeAsync<'T>       : Stream -> Task<'T>

[<RequireQualifiedAccess>]
module SystemTextJson =
    open System
    open System.IO
    open System.Text
    open System.Text.Json
    open System.Threading.Tasks

    /// <summary>
    /// <see cref="SystemTextJson.Serializer" /> is an alternaive <see cref="Json.ISerializer"/> in Oxpecker.
    ///
    /// It uses <see cref="System.Text.Json"/> as the underlying JSON serializer to (de-)serialize
    /// JSON content.
    /// For support of F# unions and records, look at https://github.com/Tarmil/FSharp.SystemTextJson
    /// which plugs into this serializer.
    /// </summary>
    type Serializer (options: JsonSerializerOptions) =

        //fix this
        static member DefaultOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)

        interface Json.ISerializer with
            member this.SerializeToString (x: 'T) =
                JsonSerializer.Serialize(x,  options)

            member this.SerializeToBytes (x: 'T) =
                JsonSerializer.SerializeToUtf8Bytes(x, options)

            member this.SerializeToStreamAsync (x: 'T) (stream: Stream) =
                JsonSerializer.SerializeAsync(stream, x, options)

            member this.Deserialize<'T> (json: string): 'T =
                JsonSerializer.Deserialize<'T>(json, options)

            member this.Deserialize<'T> (bytes: byte array): 'T =
                JsonSerializer.Deserialize<'T>(Span<_>.op_Implicit(bytes.AsSpan()), options)

            member this.DeserializeAsync<'T> (stream: Stream): Task<'T> =
                JsonSerializer.DeserializeAsync<'T>(stream, options).AsTask()