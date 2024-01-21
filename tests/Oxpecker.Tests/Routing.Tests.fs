module Oxpecker.Tests.Routing

open System
open System.IO
open System.Collections.Generic
open System.Net
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Xunit
open FsUnitTyped
open Oxpecker

module WebApp =

    let notFoundHandler = setStatusCode 404 >=> text "Not found"

    let webApp endpoints =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app -> app.UseRouting().UseOxpecker(endpoints).Run(notFoundHandler))
                .ConfigureServices(fun services -> services.AddRouting() |> ignore)
        new TestServer(builder)

// ---------------------------------
// route Tests
// ---------------------------------

[<Fact>]
let ``route: GET "/" returns "Hello World"`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World"; route "/foo" <| text "bar" ] ]
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

        let! result = client.GetAsync("/")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "Hello World"
    }

[<Fact>]
let ``route: GET "/foo" returns "bar"`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World"; route "/foo" <| text "bar" ] ]
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

        let! result = client.GetAsync("/foo")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "bar"
    }

// ---------------------------------
// routex Tests
// ---------------------------------



[<Fact>]
let ``routex: GET "/foo///" returns "bar"`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World"; route "/foo/{**path}" <| text "bar" ] ]
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

        let! result = client.GetAsync("/foo///")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "bar"
    }

[<Fact>]
let ``routex: GET "/foo2" returns "bar"`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World"; route "/foo2/{*path}" <| text "bar" ] ]
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

        let! result = client.GetAsync("/foo2")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "bar"
    }


// ---------------------------------
// routef Tests
// ---------------------------------

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
                routef "/foo/{%s}/bar" text
                routef "/foo/{%s}/{%i}" (fun name age -> text $"Name: %s{name}, Age: %i{age}")
            ]
        ]
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

        let! result = client.GetAsync("/foo/12635000945053400782/bar/16547050693006839099")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString
        |> shouldEqual "Id1: 12635000945053400782, Id2: 16547050693006839099"
    }

[<Fact>]
let ``routef: GET "/foo/bar/baz/qux" returns 404 "Not found"`` () =
    task {
        let endpoints = [ GET [ routef "/foo/%s/%s" (fun s1 s2 -> text $"%s{s1},%s{s2}") ] ]
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

        let! result = client.GetAsync("/foo/bar/baz/qux")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.NotFound
        resultString |> shouldEqual "Not found"
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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

        let! result = client.GetAsync("/api/foo/bar/yadayada")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "yadayada"
    }
