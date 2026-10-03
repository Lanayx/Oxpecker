module Oxpecker.Tests.Routing

open System
open System.Net
open System.Net.Http
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
                            .Configure(fun app -> app.UseRouting().UseOxpecker(endpoints).Run(Default.notFoundHandler))
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
                            .Configure(fun app -> app.UseRouting().UseOxpecker(endpoint).Run(Default.notFoundHandler))
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
                                _
                                    .UseRouting()
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

[<Fact>]
let ``route: QUERY "/search" returns "foo"`` () =
    task {
        let endpoints = [
            QUERY [
                route "/search" (fun ctx ->
                    task {
                        let! query = ctx.BindJson<{| Filter: string |}>()
                        return! ctx.WriteText query.Filter
                    })
            ]
        ]
        use! server = WebApp.webAppWithDefaultErrorHandler endpoints
        let client = server.GetTestClient()

        use request = new HttpRequestMessage(HttpMethod "QUERY", "/search")
        request.Content <- JsonContent.Create({| Filter = "foo" |})
        let! result = client.SendAsync request
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "foo"
    }

[<Fact>]
let ``route: GET "/search" on QUERY route returns 405 "Method Not Allowed"`` () =
    task {
        let endpoints = [ QUERY [ route "/search" <| text "foo" ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/search")

        result.StatusCode |> shouldEqual HttpStatusCode.MethodNotAllowed
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
        let endpoint = routef "/foo/{%s}/{%i}/{%O:guid}" (fun x y z -> text $"Hello {x}{y}{z}")

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
// subRoutef Tests
// ---------------------------------

[<Fact>]
let ``subRoutef generates route correctly`` () =
    let endpoint = subRoutef "/foo/{%s}/{%i}/{%O:guid}" (fun x y (z: Guid) -> [ route "/" (text $"Hello {x}{y}{z}") ])

    match endpoint with
    | NestedEndpoint(route, _, _) -> route |> shouldEqual "/foo/{x}/{y}/{z:guid}"
    | _ -> failwith "Expected NestedEndpoint"

[<Fact>]
let ``subRoutef: route inside subRoutef receives typed parameter`` () =
    task {
        let endpoints = [
            GET [
                route "/users" <| text "users"
                subRoutef "/users/{%i}" (fun userId -> [
                    route "" <| text $"User {userId + 1}"
                    route "/profile" <| text $"Profile {userId}"
                ])
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! users = client.GetStringAsync("/users")
        let! user = client.GetStringAsync("/users/42")
        let! profile1 = client.GetStringAsync("/users/42/profile")
        let! profile2 = client.GetStringAsync("/users/7/profile")

        users |> shouldEqual "users"
        user |> shouldEqual "User 43"
        profile1 |> shouldEqual "Profile 42"
        profile2 |> shouldEqual "Profile 7"
    }

[<Fact>]
let ``subRoutef: multiple parameters and routef inside subRoutef`` () =
    task {
        let endpoints = [
            GET [
                subRoutef "/orgs/{%s}/users/{%i}" (fun org userId -> [
                    route "/info" <| text $"{org}:{userId}"
                    routef "/posts/{%i}/{%O:guid}" (fun postId (commentId: Guid) ->
                        text $"{org}:{userId}:{postId}:{commentId}")
                ])
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()
        let guid = Guid.NewGuid()

        let! info = client.GetStringAsync("/orgs/acme/users/5/info")
        let! post = client.GetStringAsync($"/orgs/acme/users/5/posts/12/{guid}")

        info |> shouldEqual "acme:5"
        post |> shouldEqual $"acme:5:12:{guid}"
    }

[<Fact>]
let ``subRoutef: HTTP verbs and groups inside and outside subRoutef`` () =
    task {
        let endpoints = [
            GET [
                subRoutef "/users/{%i}" (fun userId -> [
                    route "/a" <| text $"a{userId}"
                    routeGroup [ route "/b" <| text $"b{userId}" ]
                    subRoute "/c" [ route "/d" <| text $"d{userId}" ]
                ])
            ]
            subRoutef "/items/{%s}" (fun item -> [
                GET [ route "" <| text $"get {item}" ]
                POST [ route "" <| text $"post {item}" ]
            ])
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! a = client.GetStringAsync("/users/1/a")
        let! b = client.GetStringAsync("/users/2/b")
        let! d = client.GetStringAsync("/users/3/c/d")
        let! postA = client.PostAsync("/users/1/a", null)
        let! getItem = client.GetStringAsync("/items/x")
        let! postItem = client.PostAsync("/items/y", null)
        let! postItemString = postItem.Content.ReadAsStringAsync()

        a |> shouldEqual "a1"
        b |> shouldEqual "b2"
        d |> shouldEqual "d3"
        postA.StatusCode |> shouldEqual HttpStatusCode.MethodNotAllowed
        getItem |> shouldEqual "get x"
        postItemString |> shouldEqual "post y"
    }

[<Fact>]
let ``subRoutef: addFilter and addMetadata inside and outside subRoutef`` () =
    task {
        let values = ResizeArray<string>()
        let filter (value: string) : EndpointHandler =
            fun ctx ->
                values.Add value
                Task.CompletedTask
        let endpoints = [
            GET [
                subRoutef "/users/{%i}" (fun userId -> [
                    route "/profile" (fun ctx ->
                        ctx.GetEndpoint()
                        |> Unchecked.nonNull
                        |> _.Metadata
                        |> _.GetRequiredMetadata<string>()
                        |> values.Add
                        ctx.WriteText $"Profile {userId}")
                    |> addMetadata "metadata"
                    |> addFilter(filter $"inner {userId}")
                ])
                |> addFilter(filter "outer")
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! profile = client.GetStringAsync("/users/5/profile")

        profile |> shouldEqual "Profile 5"
        values.ToArray() |> shouldEqual [| "outer"; "inner 5"; "metadata" |]
    }

[<Fact>]
let ``subRoutef: nested subRoutef`` () =
    task {
        let endpoints = [
            subRoutef "/orgs/{%s}" (fun org -> [
                route "" <| text $"Org {org}"
                subRoutef "/users/{%i}" (fun userId -> [ route "/info" <| text $"{org}:{userId}" ])
            ])
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! org = client.GetStringAsync("/orgs/acme")
        let! info1 = client.GetStringAsync("/orgs/acme/users/9/info")
        let! info2 = client.GetStringAsync("/orgs/other/users/1/info")

        org |> shouldEqual "Org acme"
        info1 |> shouldEqual "acme:9"
        info2 |> shouldEqual "other:1"
    }

let private userEndpoints (userId: int) = [ route "/profile" <| text $"Profile {userId}" ]

[<Fact>]
let ``subRoutef: module function, piped lambda and no parameters`` () =
    task {
        let endpoints = [
            subRoutef "/users/{%i}" userEndpoints
            (fun name -> [ route "/hello" <| text $"Hello {name}" ])
            |> subRoutef "/names/{%s}"
            subRoutef "/api" [ route "/test" <| text "test" ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! profile = client.GetStringAsync("/users/3/profile")
        let! hello = client.GetStringAsync("/names/John/hello")
        let! test = client.GetStringAsync("/api/test")

        profile |> shouldEqual "Profile 3"
        hello |> shouldEqual "Hello John"
        test |> shouldEqual "test"
    }

[<Fact>]
let ``subRoutef: invalid parameter value returns 400`` () =
    task {
        let endpoints = [ subRoutef "/users/{%i}" userEndpoints ]
        use! server = WebApp.webAppWithDefaultErrorHandler endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/users/abc/profile")

        result.StatusCode |> shouldEqual HttpStatusCode.BadRequest
    }

[<Fact>]
let ``subRoutef: exceptions from factory and handlers are not wrapped`` () =
    task {
        let endpoints = [
            subRoutef "/users/{%i}" (fun userId ->
                if userId < 0 then
                    raise <| NotSupportedException "factory"
                else
                    [
                        route "/a" (fun _ -> raise <| NotSupportedException "route")
                        routef "/b/{%i}" (fun (_: int) (_: HttpContext) -> raise <| NotSupportedException "routef")
                    ])
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        do! client.GetAsync("/users/-1/a") |> shouldFailTask<NotSupportedException>
        do! client.GetAsync("/users/1/a") |> shouldFailTask<NotSupportedException>
        do! client.GetAsync("/users/1/b/2") |> shouldFailTask<NotSupportedException>
    }

[<Fact>]
let ``subRoutef: endpoints depending on parameter values fail`` () =
    task {
        let endpoints = [
            subRoutef "/users/{%i}" (fun userId -> [
                if userId = 0 then
                    route "/zero" <| text "zero"
                route "/profile" <| text $"Profile {userId}"
            ])
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! ex = Assert.ThrowsAsync<InvalidOperationException>(fun () -> client.GetAsync("/users/5/profile"))

        ex.Message
        |> shouldEqual
            "subRoutef '/users/{%i}': endpoints factory returned different endpoints than at startup. Number, order, nesting and templates of endpoints must not depend on route values."
    }

[<Fact>]
let ``subRoutef: invalid definitions fail immediately`` () =
    let catchAll = Assert.Throws<Exception>(fun () -> subRoutef "/files/{**%s}" (fun (_: string) -> []) |> ignore)
    let fewerParameters =
        Assert.Throws<Exception>(fun () ->
            subRoutef "/{%s}/{%i}" (fun (a: string) ->
                Console.Write a
                fun (_: int) -> [])
            |> ignore)
    let factoryFailure =
        Assert.Throws<InvalidOperationException>(fun () ->
            subRoutef "/users/{%i}" (fun userId -> if userId = 0 then failwith "boom" else [])
            |> ignore)

    catchAll.Message
    |> shouldEqual "Catch-all parameters are not supported in subRoutef: /files/{**%s}"
    fewerParameters.Message
    |> shouldEqual "Handler has fewer parameters than route placeholders: /{%s}/{%i}"
    factoryFailure.InnerException
    |> Unchecked.nonNull
    |> _.Message
    |> shouldEqual "boom"

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
