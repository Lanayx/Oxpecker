module Oxpecker.OpenApi.Tests.General

open System.ComponentModel
open System.Net
open System.Threading.Tasks
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi
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
                                app
                                    .UseRouting()
                                    .UseEndpoints(fun builder ->
                                        builder.MapOxpeckerEndpoints(endpoints)
                                        builder.MapOpenApi() |> ignore)
                                |> ignore)
                            .ConfigureServices(fun services -> services.AddRouting().AddOpenApi() |> ignore)
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
        let endpoints = [
            POST [
                route "/" <| text "Hello World"
                |> addOpenApi(
                    OpenApiConfig(
                        requestBody = RequestBody(typeof<Request1>),
                        responseBodies = [ ResponseBody(typeof<Response1>) ]
                    )
                )
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
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
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
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
        let endpoints = [
            POST [ route "/" <| text "Hello World" |> addOpenApiSimple<unit, Response1> ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
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
        let endpoints = [ POST [ route "/" <| text "Hello World" |> addOpenApiSimple<Request1, unit> ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
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
        let endpoints = [ GET [ route "/" <| text "Hello World" |> addOpenApiSimple<unit, Response2> ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
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
        let endpoints = [
            GET [
                routef "/product/{%i}" <| fun i -> text $"Hello %i{i}"
                |> addOpenApiSimple<unit, string>
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
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

[<Fact>]
let ``subRoutef path parameters work fine`` () =
    task {
        let endpoints = [
            GET [
                subRoutef "/users/{%i}" (fun userId -> [
                    route "/profile" <| text $"Profile {userId}" |> addOpenApiSimple<unit, string>
                    routef "/posts/{%O:guid}" (fun (postId: System.Guid) -> text $"Post {userId} {postId}")
                    |> addOpenApiSimple<unit, string>
                ])
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
    }
  ],
  "paths": {
    "/users/{userId}/profile": {
      "get": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "parameters": [
          {
            "name": "userId",
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
    },
    "/users/{userId}/posts/{postId}": {
      "get": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "parameters": [
          {
            "name": "userId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int32"
            }
          },
          {
            "name": "postId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string",
              "format": "uuid"
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

[<Fact>]
let ``Path parameters don't replace other parameters with the same name`` () =
    task {
        // group transformer runs before the routef one
        let addQueryParameter (operation: OpenApiOperation) _ _ =
            let parameter =
                OpenApiParameter(
                    Name = "postId",
                    In = ParameterLocation.Query,
                    Schema = OpenApiSchema(Type = JsonSchemaType.String)
                )
            match operation.Parameters with
            | null -> operation.Parameters <- ResizeArray [ parameter :> IOpenApiParameter ]
            | parameters -> parameters.Add parameter
            Task.CompletedTask
        let endpoints = [
            GET [
                subRoutef "/users/{%i}" (fun userId -> [
                    routef "/posts/{%i}" (fun postId -> text $"Post {userId} {postId}")
                    |> addOpenApiSimple<unit, string>
                ])
                |> addOpenApi(OpenApiConfig(configureOperation = addQueryParameter))
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
    }
  ],
  "paths": {
    "/users/{userId}/posts/{postId}": {
      "get": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "parameters": [
          {
            "name": "userId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int32"
            }
          },
          {
            "name": "postId",
            "in": "query",
            "schema": {
              "type": "string"
            }
          },
          {
            "name": "postId",
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

[<Description("Type description")>]
type Response3 = {
    [<Description("Field description")>]
    Valid: bool
}

[<Fact>]
let ``Additional configuration works fine`` () =
    task {
        let endpoints = [
            POST [
                route "/" <| text "Hello World"
                |> addOpenApi(
                    OpenApiConfig(
                        responseBodies = [ ResponseBody(typeof<Response3>) ],
                        configureOperation =
                            fun operation _ _ ->
                                task {
                                    operation.Description <- "Endpoint description"
                                    return operation
                                }
                    )
                )
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
    }
  ],
  "paths": {
    "/": {
      "post": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "description": "Endpoint description",
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Response3"
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
      "Response3": {
        "required": [
          "valid"
        ],
        "type": "object",
        "properties": {
          "valid": {
            "type": "boolean",
            "description": "Field description"
          }
        },
        "description": "Type description"
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
let ``addOpenApi on group and endpoint works fine`` () =
    task {
        let endpoints = [
            GET [
                subRoute "/api" [
                    route "/a" <| text "a" |> addOpenApiSimple<unit, string>
                    route "/b" <| text "b"
                ]
                |> addOpenApi(OpenApiConfig(responseBodies = [ ResponseBody(typeof<Response1>, statusCode = 400) ]))
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
    }
  ],
  "paths": {
    "/api/a": {
      "get": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
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
          },
          "400": {
            "description": "Bad Request",
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
    },
    "/api/b": {
      "get": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "responses": {
          "400": {
            "description": "Bad Request",
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
let ``addOpenApi applied twice to endpoint works fine`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                |> addOpenApiSimple<unit, string>
                |> addOpenApi(
                    OpenApiConfig(
                        configureOperation =
                            fun operation _ _ ->
                                operation.Description <- "Endpoint description"
                                Task.CompletedTask
                    )
                )
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/openapi/v1.json")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        let expected =
            """{
  "openapi": "3.2.0",
  "info": {
    "title": "Oxpecker.OpenApi.Tests | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "http://localhost"
    }
  ],
  "paths": {
    "/": {
      "get": {
        "tags": [
          "Oxpecker.OpenApi.Tests"
        ],
        "description": "Endpoint description",
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
