module Oxpecker.OpenApi.Tests.General

open System.Net
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Oxpecker
open Oxpecker.OpenApi
open Xunit
open FsUnit.Light

module WebApp =

    let webApp (endpoints: Endpoint seq) =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(fun app ->
                                app.UseRouting().UseEndpoints(fun builder ->
                                    builder.MapOxpeckerEndpoints(endpoints)
                                    builder.MapOpenApi() |> ignore
                                ) |> ignore)
                            .ConfigureServices(fun services ->
                                services.AddRouting().AddOpenApi() |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }

type Request1 = { Name: int }
type Response1 = { Valid: bool }

[<Fact>]
let ``addOpenApi works fine`` () =
    task {
        let endpoints = [ POST [
            route "/" <| text "Hello World"
                |> addOpenApi(
                    OpenApiConfig(
                        requestBody = RequestBody(typeof<Request1>),
                        responseBodies = [ ResponseBody(typeof<Response1>) ]
                    )
                )
        ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected = """{
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
            "type": "boolean"
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

[<Fact>]
let ``addOpenApiSimple works fine`` () =
    task {
        let endpoints = [ POST [
            route "/" <| text "Hello World"
                |> addOpenApiSimple<Request1, Response1>
        ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected = """{
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
            "type": "boolean"
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


[<Fact>]
let ``addOpenApiSimple with unit request works fine`` () =
    task {
        let endpoints = [ POST [
            route "/" <| text "Hello World"
                |> addOpenApiSimple<unit, Response1>
        ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected = """{
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
      "Response1": {
        "required": [
          "valid"
        ],
        "type": "object",
        "properties": {
          "valid": {
            "type": "boolean"
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


[<Fact>]
let ``addOpenApiSimple with unit response works fine`` () =
    task {
        let endpoints = [ POST [
            route "/" <| text "Hello World"
                |> addOpenApiSimple<Request1, unit>
        ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected = """{
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
            "description": "OK"
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
              "integer",
              "string"
            ],
            "format": "int32"
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
type Response2Inner = { Valid: bool }
type Response2 = { Inner: Response2Inner }

[<Fact>]
let ``nested objects work fine`` () =
    task {
        let endpoints = [ GET [
            route "/" <| text "Hello World"
                |> addOpenApiSimple<unit, Response2>
        ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected = """{
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
            "$ref": "#/components/schemas/Response2Inner"
          }
        }
      },
      "Response2Inner": {
        "type": "object",
        "properties": {
          "valid": {
            "type": "boolean"
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


[<Fact>]
let ``Path parameter works fine`` () =
    task {
        let endpoints = [ GET [
            routef "/product/{%i}" <| fun i -> text $"Hello %i{i}"
                |> addOpenApiSimple<unit, string>
        ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected = """{
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
    "/product/{i}": {
      "get": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "parameters": [
          {
            "name": "i",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int32"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "text/plain": {
                "schema": {
                  "type": "string"
                }
              }
            }
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
