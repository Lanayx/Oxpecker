module Oxpecker.Tests.Json

open System
open System.Buffers
open System.IO
open System.IO.Pipelines
open System.Text.Json
open System.Threading
open Microsoft.AspNetCore.Http
open Microsoft.Net.Http.Headers
open Oxpecker
open Xunit
open FsUnit.Light

#nowarn "3391"

[<Fact>]
let ``Test not chunked serializer`` () =
    task {
        let serializer: IJsonSerializer = SystemTextJsonSerializer()
        let httpContext = DefaultHttpContext()
        httpContext.Response.Body <- new MemoryStream()
        let value = {| Name = "Oxpecker" |}
        do! serializer.Serialize(value, httpContext, false)
        let stream = httpContext.Response.Body
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader = new StreamReader(stream)
        let json = streamReader.ReadToEnd()
        json |> shouldEqual """{"name":"Oxpecker"}"""
        httpContext.Response.Headers.ContentType
        |> shouldEqual "application/json; charset=utf-8"
        httpContext.Response.Headers.ContentLength |> shouldEqual 19L
    }

[<Fact>]
let ``Test chunked serializer`` () =
    task {
        let serializer: IJsonSerializer = SystemTextJsonSerializer()
        let httpContext = DefaultHttpContext()
        httpContext.Response.Body <- new MemoryStream()
        let value = {| Name = "Oxpecker" |}
        do! serializer.Serialize(value, httpContext, true)
        let stream = httpContext.Response.Body
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader = new StreamReader(stream)
        let json = streamReader.ReadToEnd()
        json |> shouldEqual """{"name":"Oxpecker"}"""
        httpContext.Response.Headers.ContentType
        |> shouldEqual "application/json; charset=utf-8"
        httpContext.Response.Headers.ContentLength |> shouldEqual(Nullable())
    }

[<Fact>]
let ``Test not chunked serializer with custom options`` () =
    task {
        let options = JsonSerializerOptions(PropertyNamingPolicy = null) // Pascal case
        let serializer: IJsonSerializer = SystemTextJsonSerializer(options)
        let httpContext = DefaultHttpContext()
        httpContext.Response.Body <- new MemoryStream()
        let value = {| Name = "Oxpecker" |}
        do! serializer.Serialize(value, httpContext, false)
        let stream = httpContext.Response.Body
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader = new StreamReader(stream)
        let json = streamReader.ReadToEnd()
        json |> shouldEqual """{"Name":"Oxpecker"}"""
        httpContext.Response.Headers.ContentType
        |> shouldEqual "application/json; charset=utf-8"
        httpContext.Response.Headers.ContentLength |> shouldEqual 19L
    }

[<Fact>]
let ``Test chunked serializer with custom options`` () =
    task {
        let options = JsonSerializerOptions(PropertyNamingPolicy = null) // Pascal case
        let serializer: IJsonSerializer = SystemTextJsonSerializer(options)
        let httpContext = DefaultHttpContext()
        httpContext.Response.Body <- new MemoryStream()
        let value = {| Name = "Oxpecker" |}
        do! serializer.Serialize(value, httpContext, true)
        let stream = httpContext.Response.Body
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader = new StreamReader(stream)
        let json = streamReader.ReadToEnd()
        json |> shouldEqual """{"Name":"Oxpecker"}"""
        httpContext.Response.Headers.ContentType
        |> shouldEqual "application/json; charset=utf-8"
        httpContext.Response.Headers.ContentLength |> shouldEqual(Nullable())
    }

[<Fact>]
let ``Test default deserializer`` () =
    task {
        let serializer: IJsonSerializer = SystemTextJsonSerializer()
        let httpContext = DefaultHttpContext()
        httpContext.Request.Body <- new MemoryStream()
        httpContext.Request.Headers[HeaderNames.ContentType] <- "application/json; charset=utf-8"
        use streamWriter = new StreamWriter(httpContext.Request.Body)
        streamWriter.Write("""{"name":"Oxpecker"}""")
        streamWriter.Flush()
        httpContext.Request.Body.Seek(0L, SeekOrigin.Begin) |> ignore
        let! value = serializer.Deserialize<{| Name: string |}>(httpContext)
        value |> shouldEqual {| Name = "Oxpecker" |}
    }

[<Fact>]
let ``Test default deserializer with custom options`` () =
    task {
        let options = JsonSerializerOptions(PropertyNamingPolicy = null) // Pascal case
        let serializer: IJsonSerializer = SystemTextJsonSerializer(options)
        let httpContext = DefaultHttpContext()
        httpContext.Request.Body <- new MemoryStream()
        httpContext.Request.Headers[HeaderNames.ContentType] <- "application/json; charset=utf-8"
        use streamWriter = new StreamWriter(httpContext.Request.Body)
        streamWriter.Write("""{"Name":"Oxpecker"}""")
        streamWriter.Flush()
        httpContext.Request.Body.Seek(0L, SeekOrigin.Begin) |> ignore
        let! value = serializer.Deserialize<{| Name: string |}>(httpContext)
        value |> shouldEqual {| Name = "Oxpecker" |}
    }

[<Fact>]
let ``Test default deserializer with nullables`` () =
    task {
        let serializer: IJsonSerializer = SystemTextJsonSerializer()
        let httpContext = DefaultHttpContext()
        httpContext.Request.Body <- new MemoryStream()
        httpContext.Request.Headers[HeaderNames.ContentType] <- "application/json; charset=utf-8"
        use streamWriter = new StreamWriter(httpContext.Request.Body)
        streamWriter.Write("""{"name":"Oxpecker"}""")
        streamWriter.Flush()
        httpContext.Request.Body.Seek(0L, SeekOrigin.Begin) |> ignore
        let! value =
            serializer.Deserialize<
                {|
                    Name: string | null
                    Age: Nullable<int>
                    Title: string | null
                |}
             >(
                httpContext
            )
        value
        |> shouldEqual {|
            Name = "Oxpecker"
            Age = Nullable()
            Title = null
        |}
    }

[<Fact>]
let ``Test part serializer`` () =
    task {
        let serializer: IJsonSerializer = SystemTextJsonSerializer()
        use stream = new MemoryStream()
        let writer = PipeWriter.Create(stream, StreamPipeWriterOptions(leaveOpen = true))
        let value = {| Name = "Oxpecker" |}
        do! serializer.SerializePart(value, writer, CancellationToken.None)
        // the writer is still usable afterwards, i.e. the serializer has not completed it
        writer.Write(ReadOnlySpan "!"B)
        let! _ = writer.FlushAsync()
        writer.Complete()
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader = new StreamReader(stream)
        let json = streamReader.ReadToEnd()
        json |> shouldEqual """{"name":"Oxpecker"}!"""
    }

[<Fact>]
let ``Test part serializer with custom options`` () =
    task {
        let options = JsonSerializerOptions(PropertyNamingPolicy = null) // Pascal case
        let serializer: IJsonSerializer = SystemTextJsonSerializer(options)
        use stream = new MemoryStream()
        let writer = PipeWriter.Create(stream, StreamPipeWriterOptions(leaveOpen = true))
        let value = {| Name = "Oxpecker" |}
        do! serializer.SerializePart(value, writer, CancellationToken.None)
        writer.Complete()
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader = new StreamReader(stream)
        let json = streamReader.ReadToEnd()
        json |> shouldEqual """{"Name":"Oxpecker"}"""
    }

[<Fact>]
let ``Test part serializer with cancelled token`` () =
    task {
        let serializer: IJsonSerializer = SystemTextJsonSerializer()
        use cts = new CancellationTokenSource()
        cts.Cancel()
        let writer = PipeWriter.Create(new MemoryStream())
        let! _ =
            Assert.ThrowsAnyAsync<OperationCanceledException>(fun () ->
                serializer.SerializePart({| Name = "Oxpecker" |}, writer, cts.Token))
        ()
    }
