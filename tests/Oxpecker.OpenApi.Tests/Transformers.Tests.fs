module Oxpecker.OpenApi.Tests.Transformers

open System
open System.Net
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Oxpecker.OpenApi
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
                                app
                                    .UseRouting()
                                    .UseEndpoints(fun builder ->
                                        builder.MapOxpeckerEndpoints(endpoints)
                                        builder.MapOpenApi() |> ignore)
                                |> ignore)
                            .ConfigureServices(fun services ->
                                services
                                    .AddRouting()
                                    .AddOpenApi(fun o ->
                                        o.AddSchemaTransformer<FSharpOptionSchemaTransformer>() |> ignore)
                                |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }
    let webAppCreateSchemaReferenceId (endpoints: Endpoint seq) =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(fun app ->
                                app
                                    .UseRouting()
                                    .UseEndpoints(fun builder ->
                                        builder.MapOxpeckerEndpoints(endpoints)
                                        builder.MapOpenApi() |> ignore)
                                |> ignore)
                            .ConfigureServices(fun services ->
                                services
                                    .AddRouting()
                                    .AddOpenApi(fun o ->
                                        o.AddSchemaTransformer<FSharpOptionSchemaTransformer>() |> ignore
                                        o.CreateSchemaReferenceId <- _.Type.FullName)
                                |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }

type Request1 = { Name: int voption }
type Response1 = { Valid: bool option }

[<Fact>]
let ``Option and voption on primitive types works fine`` () =
    task {
        let endpoints = [
            POST [ route "/" <| text "Hello World" |> addOpenApiSimple<Request1, Response1> ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.1.1",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost/"
    }
  ],
  "paths": {
    "/": {
      "post": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/Request1"
              }
            }
          },
          "required": true
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Response1"
                }
              }
            }
          }
        }
      }
    }
  },
  "components": {
    "schemas": {
      "Request1": {
        "required": [
          "name"
        ],
        "type": "object",
        "properties": {
          "name": {
            "pattern": "^-?(?:0|[1-9]\\d*)$",
            "type": [
              "null",
              "integer",
              "string"
            ],
            "format": "int32"
          }
        }
      },
      "Response1": {
        "required": [
          "valid"
        ],
        "type": "object",
        "properties": {
          "valid": {
            "type": [
              "null",
              "boolean"
            ]
          }
        }
      }
    }
  },
  "tags": [
    {
      "name": "Oxpecker.OpenApi.Tests"
    }
  ]
}"""
        resultString.ReplaceLineEndings() |> shouldEqual expected
    }


[<CLIMutable>]
type Response2Inner = { Valid: bool voption }
type Response2 = { Inner: Response2Inner option }

[<Fact>]
let ``nested objects with options work fine`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World" |> addOpenApiSimple<unit, Response2> ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.1.1",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost/"
    }
  ],
  "paths": {
    "/": {
      "get": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Response2"
                }
              }
            }
          }
        }
      }
    }
  },
  "components": {
    "schemas": {
      "Response2": {
        "required": [
          "inner"
        ],
        "type": "object",
        "properties": {
          "inner": {
            "oneOf": [
              {
                "type": "null"
              },
              {
                "$ref": "#/components/schemas/Response2Inner"
              }
            ]
          }
        }
      },
      "Response2Inner": {
        "type": "object",
        "properties": {
          "valid": {
            "type": [
              "null",
              "boolean"
            ]
          }
        }
      }
    }
  },
  "tags": [
    {
      "name": "Oxpecker.OpenApi.Tests"
    }
  ]
}"""
        resultString.ReplaceLineEndings() |> shouldEqual expected
    }


type Request3 = { apple: bool option }
type Response3 = { banana: bool }

[<Fact>]
let ``Issue 87 CreateSchemaReferenceId works well`` () =
    task {
        let endpoints = [
            GET [ route "/" <| text "Hello World" |> addOpenApiSimple<Request3, Response3> ]
        ]
        use! server = WebApp.webAppCreateSchemaReferenceId endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.1.1",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost/"
    }
  ],
  "paths": {
    "/": {
      "get": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/Oxpecker.OpenApi.Tests.Transformers+Request3"
              }
            }
          },
          "required": true
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Oxpecker.OpenApi.Tests.Transformers+Response3"
                }
              }
            }
          }
        }
      }
    }
  },
  "components": {
    "schemas": {
      "Oxpecker.OpenApi.Tests.Transformers+Request3": {
        "required": [
          "apple"
        ],
        "type": "object",
        "properties": {
          "apple": {
            "oneOf": [
              {
                "type": "null"
              },
              {
                "$ref": "#/components/schemas/System.Boolean"
              }
            ]
          }
        }
      },
      "Oxpecker.OpenApi.Tests.Transformers+Response3": {
        "required": [
          "banana"
        ],
        "type": "object",
        "properties": {
          "banana": {
            "$ref": "#/components/schemas/System.Boolean"
          }
        }
      },
      "System.Boolean": {
        "type": "boolean"
      }
    }
  },
  "tags": [
    {
      "name": "Oxpecker.OpenApi.Tests"
    }
  ]
}"""
        resultString.ReplaceLineEndings() |> shouldEqual expected
    }
