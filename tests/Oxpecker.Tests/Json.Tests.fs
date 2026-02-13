module Oxpecker.Tests.Json

open System
open System.IO
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
let ``Test custom JsonSerializerOptions are used in non-chunked serialization`` () =
    task {
        // Use PascalCase naming policy (null) instead of default camelCase
        let customOptions = System.Text.Json.JsonSerializerOptions()
        customOptions.PropertyNamingPolicy <- null
        let serializer: IJsonSerializer = SystemTextJsonSerializer(customOptions)
        let httpContext = DefaultHttpContext()
        httpContext.Response.Body <- new MemoryStream()
        let value = {| FirstName = "John"; LastName = "Doe" |}
        do! serializer.Serialize(value, httpContext, false)
        let stream = httpContext.Response.Body
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader = new StreamReader(stream)
        let json = streamReader.ReadToEnd()
        // Should use PascalCase (FirstName, LastName) not camelCase (firstName, lastName)
        json |> shouldEqual """{"FirstName":"John","LastName":"Doe"}"""
    }

[<Fact>]
let ``Test custom JsonSerializerOptions are used in chunked serialization`` () =
    task {
        // Use PascalCase naming policy (null) instead of default camelCase
        let customOptions = System.Text.Json.JsonSerializerOptions()
        customOptions.PropertyNamingPolicy <- null
        let serializer: IJsonSerializer = SystemTextJsonSerializer(customOptions)
        let httpContext = DefaultHttpContext()
        httpContext.Response.Body <- new MemoryStream()
        let value = {| FirstName = "John"; LastName = "Doe" |}
        do! serializer.Serialize(value, httpContext, true)
        let stream = httpContext.Response.Body
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader = new StreamReader(stream)
        let json = streamReader.ReadToEnd()
        // Should use PascalCase (FirstName, LastName) not camelCase (firstName, lastName)
        json |> shouldEqual """{"FirstName":"John","LastName":"Doe"}"""
    }

[<Fact>]
let ``Test chunked and non-chunked serialization produce consistent output with custom options`` () =
    task {
        let customOptions = System.Text.Json.JsonSerializerOptions()
        customOptions.PropertyNamingPolicy <- null
        customOptions.WriteIndented <- true
        let serializer: IJsonSerializer = SystemTextJsonSerializer(customOptions)

        // Test non-chunked
        let httpContext1 = DefaultHttpContext()
        httpContext1.Response.Body <- new MemoryStream()
        let value = {| Status = "Active"; Count = 42 |}
        do! serializer.Serialize(value, httpContext1, false)
        let stream1 = httpContext1.Response.Body
        stream1.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader1 = new StreamReader(stream1)
        let json1 = streamReader1.ReadToEnd()

        // Test chunked
        let httpContext2 = DefaultHttpContext()
        httpContext2.Response.Body <- new MemoryStream()
        do! serializer.Serialize(value, httpContext2, true)
        let stream2 = httpContext2.Response.Body
        stream2.Seek(0L, SeekOrigin.Begin) |> ignore
        use streamReader2 = new StreamReader(stream2)
        let json2 = streamReader2.ReadToEnd()

        // Both should produce identical JSON with PascalCase and indentation
        json1 |> shouldEqual json2
        // Verify PascalCase (not camelCase)
        json1.Contains("Status") |> shouldEqual true
        json1.Contains("Count") |> shouldEqual true
        json1.Contains("status") |> shouldEqual false
        json1.Contains("count") |> shouldEqual false
    }
