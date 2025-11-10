module Oxpecker.Tests.Routing

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Xunit
open FsUnit.Light
open Oxpecker

module WebApp =

    let webApp (endpoints: Endpoint seq) =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(fun app ->
                                app.UseRouting().UseOxpecker(endpoints).Run(Default.notFoundHandler))
                            .ConfigureServices(fun services -> services.AddRouting() |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }

    let webAppOneRoute (endpoint: Endpoint) =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(fun app ->
                                app.UseRouting().UseOxpecker(endpoint).Run(Default.notFoundHandler))
                            .ConfigureServices(fun services -> services.AddRouting() |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }

    let webAppWithDefaultErrorHandler (endpoints: Endpoint seq) =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(
                                _.UseRouting()
                                    .Use(Default.exceptionMiddleware)
                                    .UseOxpecker(endpoints)
                                    .Run(Default.notFoundHandler)
                            )
                            .ConfigureServices(fun services -> services.AddRouting().AddOxpecker() |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }

// ---------------------------------
// route Tests
// ---------------------------------

[<Fact>]
let ``route: GET "/" returns "Hello World"`` () =
    task {
        let endpoint = GET [ route "/" <| text "Hello World"; route "/foo" <| text "bar" ]
        use! server = WebApp.webAppOneRoute endpoint
        let client = server.GetTestClient()

        let! result = client.GetAsync("/")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "Hello World"
    }

[<Fact>]
let ``route: GET "/foo" returns "bar"`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World"; route "/foo" <| text "bar" ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "bar"
    }

[<Fact>]
let ``Mixed GET and POST routes are not allowed`` () =
    task {
        let endpoint = GET [ POST [ route "/abc" <| text "Hello World" ] ]
        do!
            task { return! WebApp.webAppOneRoute endpoint }
            |> shouldFailTaskWithMessage "Http verbs intersect at '/abc'"
    }

// ---------------------------------
// routex Tests
// ---------------------------------

[<Fact>]
let ``routex: GET "/foo///" returns "bar"`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World"; route "/foo/{**path}" <| text "bar" ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo///")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "bar"
    }

[<Fact>]
let ``routex: GET "/foo2" returns "bar"`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World"; route "/foo2/{*path}" <| text "bar" ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo2")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "bar"
    }


// ---------------------------------
// routef Tests
// ---------------------------------

[<Fact>]
let ``routef generates route correctly`` () =
    task {
        let endpoint =
            routef "/foo/{%s}/{%i}/{%O:guid}" (fun x y z -> text $"Hello {x}{y}{z}")

        match endpoint with
        | SimpleEndpoint(_, route, _, _) -> route |> shouldEqual "/foo/{x}/{y}/{z:guid}"
        | _ -> failwith "Expected SimpleEndpoint"
    }


[<Fact>]
let ``routef: GET "/foo/blah blah/bar" returns "blah blah"`` () =

    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                routef "/foo/{%s}/bar" text
                routef "/foo/{%s}/{%i}" (fun name age -> text $"Name: %s{name}, Age: %i{age}")
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo/blah blah/bar")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "blah blah"
    }

[<Fact>]
let ``routef: GET "/foo/johndoe/59" returns "Name: johndoe, Age: 59"`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                routef "/foo/{%s}/bar" text
                routef "/foo/{%s}/{%i}" (fun name age -> text $"Name: %s{name}, Age: %i{age}")
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo/johndoe/59")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "Name: johndoe, Age: 59"
    }

[<Fact>]
let ``routef: GET "/foo/b%2Fc/bar" returns "b%2Fc"`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                routef "/foo/{%s}/bar" text
                routef "/foo/{%s}/{%i}" (fun name age -> text $"Name: %s{name}, Age: %i{age}")
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo/b%2Fc/bar")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "b/c"
    }

[<Fact>]
let ``routef: GET "/foo/a%2Fb%2Bc.d%2Ce/bar" returns "a/b+c.d,e"`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                routef "/foo/{%s}/bar" (fun name ctx -> ctx.WriteText(name))
                routef "/foo/{%s}/{%i}" (fun name age -> text $"Name: %s{name}, Age: %i{age}")
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo/a%2Fb%2Bc.d%2Ce/bar")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "a/b+c.d,e"
    }


[<Fact>]
let ``routef: GET "/foo/%O/bar/%O" returns "Guid1: ..., Guid2: ..."`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                routef "/foo/{%s}/bar" text
                routef "/foo/{%s}/{%i}" (fun name age -> text $"Name: %s{name}, Age: %i{age}")
                routef "/foo/{%O:guid}/bar/{%O:guid}" (fun (guid1: Guid) (guid2: Guid) ->
                    text $"Guid1: %O{guid1}, Guid2: %O{guid2}")
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo/4ec87f064d1e41b49342ab1aead1f99d/bar/2a6c9185-95d9-4d8c-80a6-575f99c2a716")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString
        |> shouldEqual "Guid1: 4ec87f06-4d1e-41b4-9342-ab1aead1f99d, Guid2: 2a6c9185-95d9-4d8c-80a6-575f99c2a716"
    }

[<Fact>]
let ``routef: GET "/foo/%u/bar/%u" returns "Id1: ..., Id2: ..."`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                routef "/foo/{%s}/bar" text
                routef "/foo/{%s}/{%i}" (fun name age -> text $"Name: %s{name}, Age: %i{age}")
                routef "/foo/{%u}/bar/{%u}" (fun (id1: uint64) (id2: uint64) -> text $"Id1: %u{id1}, Id2: %u{id2}")
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo/12635000945053400782/bar/16547050693006839099")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString
        |> shouldEqual "Id1: 12635000945053400782, Id2: 16547050693006839099"
    }

[<Fact>]
let ``routef: GET "/foo/bar/baz/qux" returns 404 "Not found"`` () =
    task {
        let endpoints = [ GET [ routef "/foo/{%s}/{%s}" (fun s1 s2 -> text $"%s{s1},%s{s2}") ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo/bar/baz/qux")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.NotFound
        resultString |> shouldEqual "Page not found"
    }

[<Fact>]
let ``Error in route binding leads to 400 error when default error handler is used`` () =
    task {
        let endpoints = [ GET [ routef "/invalid/{%i}" (fun i -> text $"%i{i}") ] ]
        use! server = WebApp.webAppWithDefaultErrorHandler endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/invalid/zz")
        result.StatusCode |> shouldEqual HttpStatusCode.BadRequest
    }

[<Fact>]
let ``Error in model binding leads to 400 error when default error handler is used`` () =
    task {
        let endpoints = [
            POST [
                routef "/invalid" <| bindForm(fun (_: {| Count: int |}) -> text "bind succeded")
            ]
        ]
        use! server = WebApp.webAppWithDefaultErrorHandler endpoints
        let client = server.GetTestClient()

        let! result = client.PostAsJsonAsync("/invalid", {| Count = "zz" |})
        result.StatusCode |> shouldEqual HttpStatusCode.BadRequest
    }

[<Fact>]
let ``routef: GET "/foo/bar/baz/qux" returns "bar/baz/qux"`` () =
    task {
        let endpoints = [ GET [ routef "/foo/{**%s}" text; routef "/moo/{*%s}" text ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result1 = client.GetAsync("/foo/bar/baz/qux")
        let! result1String = result1.Content.ReadAsStringAsync()

        let! result2 = client.GetAsync("/moo/bar/baz/qux")
        let! result2String = result2.Content.ReadAsStringAsync()

        result1.StatusCode |> shouldEqual HttpStatusCode.OK
        result2.StatusCode |> shouldEqual HttpStatusCode.OK
        result1String |> shouldEqual "bar/baz/qux"
        result2String |> shouldEqual "bar/baz/qux"
    }

// ---------------------------------
// subRoute Tests
// ---------------------------------

[<Fact>]
let ``subRoute: Route with empty route`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                subRoute "/api" [
                    route "" <| text "api root"
                    route "/admin" <| text "admin"
                    route "/users" <| text "users"
                ]
                route "/api/test" <| text "test"
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/api")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "api root"
    }

[<Fact>]
let ``subRoute: Normal nested route after subRoute`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                subRoute "/api" [
                    route "" <| text "api root"
                    route "/admin" <| text "admin"
                    route "/users" <| text "users"
                ]
                route "/api/test" <| text "test"
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/api/users")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "users"
    }

[<Fact>]
let ``subRoute: Route after subRoute has same beginning of path`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                subRoute "/api" [
                    route "" <| text "api root"
                    route "/admin" <| text "admin"
                    route "/users" <| text "users"
                ]
                route "/api/test" <| text "test"
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/api/test")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "test"
    }

[<Fact>]
let ``subRoute: Nested sub routes`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                subRoute "/api" [
                    route "" <| text "api root"
                    route "/admin" <| text "admin"
                    route "/users" <| text "users"
                    subRoute "/v2" [
                        route "" <| text "api root v2"
                        route "/admin" <| text "admin v2"
                        route "/users" <| text "users v2"
                    ]
                ]
                route "/api/test" <| text "test"
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/api/v2/users")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "users v2"
    }

[<Fact>]
let ``subRoute: Multiple nested sub routes`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                subRoute "/api" [
                    route "/users" <| text "users"
                    subRoute "/v2" [ route "/admin" <| text "admin v2"; route "/users" <| text "users v2" ]
                    subRoute "/v2" [ route "/admin2" <| text "correct admin2" ]
                ]
                route "/api/test" <| text "test"
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/api/v2/admin2")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "correct admin2"
    }

[<Fact>]
let ``subRoute: Route after nested sub routes has same beginning of path`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                subRoute "/api" [
                    route "" <| text "api root"
                    route "/admin" <| text "admin"
                    route "/users" <| text "users"
                    subRoute "/v2" [
                        route "" <| text "api root v2"
                        route "/admin" <| text "admin v2"
                        route "/users" <| text "users v2"
                    ]
                    route "/yada" <| text "yada"
                ]
                route "/api/test" <| text "test"
                route "/api/v2/else" <| text "else"
            ]
        ]
        let! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/api/v2/else")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "else"
    }

[<Fact>]
let ``subRoute: routef inside subRoute`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                route "/foo" <| text "bar"
                subRoute "/api" [ route "" <| text "api root"; routef "/foo/bar/{%s}" text ]
                route "/api/test" <| text "test"
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/api/foo/bar/yadayada")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "yadayada"
    }

[<Fact>]
let ``subRoute: configureEndpoint inside subRoute`` () =
    task {
        let mutable rootMetadata = Unchecked.defaultof<EndpointMetadataCollection>
        let mutable getMetadata = Unchecked.defaultof<EndpointMetadataCollection>
        let mutable innerMetadata = Unchecked.defaultof<EndpointMetadataCollection>
        let endpoints = [
            route "/" (fun ctx ->
                rootMetadata <- ctx.GetEndpoint() |> Unchecked.nonNull |> _.Metadata
                ctx.WriteText "")
            GET [
                route "/get" (fun ctx ->
                    getMetadata <- ctx.GetEndpoint() |> Unchecked.nonNull |> _.Metadata
                    ctx.WriteText "Hello World")
                subRoute "/api" [
                    routef "/inner" (fun ctx ->
                        innerMetadata <- ctx.GetEndpoint() |> Unchecked.nonNull |> _.Metadata
                        ctx.WriteText "Hi")
                ]
                |> configureEndpoint _.ShortCircuit()
            ]
            |> configureEndpoint _.DisableAntiforgery()
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! _ = client.GetAsync("/api/inner")
        let! _ = client.GetAsync("/")
        let! _ = client.GetAsync("/get")

        innerMetadata.Count |> shouldBeGreaterThan getMetadata.Count
        getMetadata.Count |> shouldBeGreaterThan rootMetadata.Count
    }

// ---------------------------------
// routeGroup Tests
// ---------------------------------

[<Fact>]
let ``routeGroup: Route group inside HTTP group`` () =
    task {
        let values = ResizeArray<string>()
        let filter: EndpointHandler =
            fun ctx ->
                values.Add "111"
                Task.CompletedTask
        let endpoints = [
            GET [
                routeGroup [
                    route "/api" (fun ctx ->
                        ctx.GetEndpoint()
                        |> Unchecked.nonNull
                        |> _.Metadata
                        |> _.GetRequiredMetadata<string>()
                        |> values.Add
                        ctx.WriteText "api root")
                ]
                |> addMetadata "222"
                |> addFilter filter
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/api")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "api root"
        values.ToArray() |> shouldEqual [| "111"; "222" |]
    }

[<Fact>]
let ``routeGroup: HTTP group inside route group `` () =
    task {
        let values = ResizeArray<string>()
        let filter: EndpointHandler =
            fun ctx ->
                values.Add "111"
                Task.CompletedTask
        let endpoints =
            routeGroup [
                GET [
                    route "/api" (fun ctx ->
                        ctx.GetEndpoint()
                        |> Unchecked.nonNull
                        |> _.Metadata
                        |> _.GetRequiredMetadata<string>()
                        |> values.Add
                        ctx.WriteText "api root")
                ]
            ]
            |> addMetadata "222"
            |> addFilter filter

        use! server = WebApp.webAppOneRoute endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/api")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "api root"
        values.ToArray() |> shouldEqual [| "111"; "222" |]
    }
