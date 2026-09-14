module Oxpecker.OpenApi.Tests.Transformers

open System
open System.ComponentModel
open System.ComponentModel.DataAnnotations
open System.Net
open System.Text.Json
open System.Text.Json.Serialization
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
                                        o
                                            .AddSchemaTransformer<FSharpOptionSchemaTransformer>()
                                            .AddSchemaTransformer<FSharpUnionSchemaTransformer>()
                                        |> ignore)
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

    let webAppInline (endpoints: Endpoint seq) =
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
                                        o
                                            .AddSchemaTransformer<FSharpOptionSchemaTransformer>()
                                            .AddSchemaTransformer<FSharpUnionSchemaTransformer>()
                                        |> ignore
                                        o.CreateSchemaReferenceId <- fun _ -> null)
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

type Color =
    | Red
    | Green
    | Blue

type Shape =
    | Point
    | Circle of radius: float
    | Rect of width: float * height: float * label: string option
    | Node of left: Shape * name: string

type Request5 = { Shape: Shape; Color: Color }

[<Fact>]
let ``F# unions are represented according to STJ union serialization format`` () =
    task {
        let endpoints = [
            POST [ route "/" <| text "Hello World" |> addOpenApiSimple<Request5, Color> ]
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
                "$ref": "#/components/schemas/Request5"
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
                  "$ref": "#/components/schemas/Color"
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
      "Color": {
        "enum": [
          "red",
          "green",
          "blue"
        ],
        "type": "string"
      },
      "Request5": {
        "required": [
          "shape",
          "color"
        ],
        "type": "object",
        "properties": {
          "shape": {
            "$ref": "#/components/schemas/Shape"
          },
          "color": {
            "$ref": "#/components/schemas/Color"
          }
        }
      },
      "Shape": {
        "oneOf": [
          {
            "const": "point",
            "type": "string"
          },
          {
            "required": [
              "$type",
              "radius"
            ],
            "type": "object",
            "properties": {
              "$type": {
                "const": "circle",
                "type": "string"
              },
              "radius": {
                "pattern": "^-?(?:0|[1-9]\\d*)(?:\\.\\d+)?(?:[eE][+-]?\\d+)?$",
                "type": [
                  "number",
                  "string"
                ],
                "format": "double"
              }
            }
          },
          {
            "required": [
              "$type",
              "width",
              "height",
              "label"
            ],
            "type": "object",
            "properties": {
              "$type": {
                "const": "rect",
                "type": "string"
              },
              "width": {
                "pattern": "^-?(?:0|[1-9]\\d*)(?:\\.\\d+)?(?:[eE][+-]?\\d+)?$",
                "type": [
                  "number",
                  "string"
                ],
                "format": "double"
              },
              "height": {
                "pattern": "^-?(?:0|[1-9]\\d*)(?:\\.\\d+)?(?:[eE][+-]?\\d+)?$",
                "type": [
                  "number",
                  "string"
                ],
                "format": "double"
              },
              "label": {
                "type": [
                  "null",
                  "string"
                ]
              }
            }
          },
          {
            "required": [
              "$type",
              "left",
              "name"
            ],
            "type": "object",
            "properties": {
              "$type": {
                "const": "node",
                "type": "string"
              },
              "left": {
                "$ref": "#/components/schemas/Shape"
              },
              "name": {
                "type": "string"
              }
            }
          }
        ]
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
[<Description("Inner type description")>]
type Response4Inner = {
    [<Description("Simple field description")>]
    Valid: bool voption
}
[<Description("Outer type description")>]
type Response4 = {
    [<Description("Nested field description")>]
    Inner: Response4Inner option
}

[<Fact>]
let ``Additional configuration works fine`` () =
    task {
        let endpoints = [
            GET [
                route "/" <| text "Hello World"
                |> addOpenApi(
                    OpenApiConfig(
                        responseBodies = [ ResponseBody(typeof<Response4>) ],
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
      "get": {
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
                  "$ref": "#/components/schemas/Response4"
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
      "Response4": {
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
                "description": "Nested field description",
                "$ref": "#/components/schemas/Response4Inner"
              }
            ]
          }
        },
        "description": "Outer type description"
      },
      "Response4Inner": {
        "type": "object",
        "properties": {
          "valid": {
            "type": [
              "null",
              "boolean"
            ],
            "description": "Simple field description"
          }
        },
        "description": "Inner type description"
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

[<JsonConverter(typeof<FSharpUnionConverterCustom>)>]
type CustomUnion =
    | Alpha
    | Beta of value: int

// A user converter whose name shares the prefix of the runtime System.Text.Json union converter
and FSharpUnionConverterCustom() =
    inherit JsonConverter<CustomUnion>()
    override _.Read(_, _, _) = Alpha
    override _.Write(writer, value, _) =
        writer.WriteStringValue(
            match value with
            | Alpha -> "alpha"
            | Beta v -> $"beta:{v}"
        )

[<Fact>]
let ``Unions with a custom converter keep the default schema`` () =
    task {
        let endpoints = [
            GET [ route "/" <| text "Hello World" |> addOpenApiSimple<unit, CustomUnion> ]
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
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/CustomUnion"
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
      "CustomUnion": { }
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

[<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
type Maybe =
    | Nothing
    | Just of value: int

[<Fact>]
let ``Nullary case of a UseNullAsTrueValue union is documented as null`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World" |> addOpenApiSimple<unit, Maybe> ] ]
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
                  "$ref": "#/components/schemas/Maybe"
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
      "Maybe": {
        "oneOf": [
          {
            "type": "null"
          },
          {
            "required": [
              "$type",
              "value"
            ],
            "type": "object",
            "properties": {
              "$type": {
                "const": "just",
                "type": "string"
              },
              "value": {
                "pattern": "^-?(?:0|[1-9]\\d*)$",
                "type": [
                  "integer",
                  "string"
                ],
                "format": "int32"
              }
            }
          }
        ]
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

[<JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)>]
type StrictUnion =
    | Empty
    | Filled of x: int

[<JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)>]
type StrictSingle = Only of y: int

type Request7 = {
    Union: StrictUnion
    Single: StrictSingle
}

[<Fact>]
let ``Disallowed unmapped members forbid additional properties on union objects`` () =
    task {
        let endpoints = [
            POST [ route "/" <| text "Hello World" |> addOpenApiSimple<Request7, StrictSingle> ]
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
                "$ref": "#/components/schemas/Request7"
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
                  "$ref": "#/components/schemas/StrictSingle"
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
      "Request7": {
        "required": [
          "union",
          "single"
        ],
        "type": "object",
        "properties": {
          "union": {
            "$ref": "#/components/schemas/StrictUnion"
          },
          "single": {
            "$ref": "#/components/schemas/StrictSingle"
          }
        }
      },
      "StrictSingle": {
        "required": [
          "$type",
          "y"
        ],
        "type": "object",
        "properties": {
          "$type": {
            "const": "only",
            "type": "string"
          },
          "y": {
            "pattern": "^-?(?:0|[1-9]\\d*)$",
            "type": [
              "integer",
              "string"
            ],
            "format": "int32"
          }
        },
        "additionalProperties": false
      },
      "StrictUnion": {
        "oneOf": [
          {
            "const": "empty",
            "type": "string"
          },
          {
            "required": [
              "$type",
              "x"
            ],
            "type": "object",
            "properties": {
              "$type": {
                "const": "filled",
                "type": "string"
              },
              "x": {
                "pattern": "^-?(?:0|[1-9]\\d*)$",
                "type": [
                  "integer",
                  "string"
                ],
                "format": "int32"
              }
            },
            "additionalProperties": false
          }
        ]
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

type NullableUnion =
    | Named of name: (string | null)
    | Anon

[<Fact>]
let ``Nullable reference fields of union cases are documented as nullable`` () =
    task {
        let endpoints = [
            GET [ route "/" <| text "Hello World" |> addOpenApiSimple<unit, NullableUnion> ]
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
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/NullableUnion"
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
      "NullableUnion": {
        "oneOf": [
          {
            "required": [
              "$type",
              "name"
            ],
            "type": "object",
            "properties": {
              "$type": {
                "const": "named",
                "type": "string"
              },
              "name": {
                "type": [
                  "null",
                  "string"
                ]
              }
            }
          },
          {
            "const": "anon",
            "type": "string"
          }
        ]
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

[<JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")>]
type Vehicle =
    | [<JsonPropertyName("bike")>] Bicycle
    | [<JsonPropertyName("auto")>] Car of seats: int
    | Truck of load: float

[<Fact>]
let ``Custom discriminator name and case name overrides are respected`` () =
    task {
        let endpoints = [ GET [ route "/" <| text "Hello World" |> addOpenApiSimple<unit, Vehicle> ] ]
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
                  "$ref": "#/components/schemas/Vehicle"
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
      "Vehicle": {
        "oneOf": [
          {
            "const": "bike",
            "type": "string"
          },
          {
            "required": [
              "kind",
              "seats"
            ],
            "type": "object",
            "properties": {
              "kind": {
                "const": "auto",
                "type": "string"
              },
              "seats": {
                "pattern": "^-?(?:0|[1-9]\\d*)$",
                "type": [
                  "integer",
                  "string"
                ],
                "format": "int32"
              }
            }
          },
          {
            "required": [
              "kind",
              "load"
            ],
            "type": "object",
            "properties": {
              "kind": {
                "const": "truck",
                "type": "string"
              },
              "load": {
                "pattern": "^-?(?:0|[1-9]\\d*)(?:\\.\\d+)?(?:[eE][+-]?\\d+)?$",
                "type": [
                  "number",
                  "string"
                ],
                "format": "double"
              }
            }
          }
        ]
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

type NullableFields = {
    A: Maybe
    B: Maybe option
    C: Color option
}
type NullableUnionFields = Fields of m: Maybe option * n: (Color | null) * c: Color option
type Request8 = {
    Record: NullableFields
    Union: NullableUnionFields
}

[<Fact>]
let ``Null-represented unions are not wrapped in a nullable oneOf`` () =
    task {
        let endpoints = [ POST [ route "/" <| text "Hello World" |> addOpenApiSimple<Request8, unit> ] ]
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
                "$ref": "#/components/schemas/Request8"
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
      "Color": {
        "enum": [
          "red",
          "green",
          "blue"
        ],
        "type": "string"
      },
      "Maybe": {
        "oneOf": [
          {
            "type": "null"
          },
          {
            "required": [
              "$type",
              "value"
            ],
            "type": "object",
            "properties": {
              "$type": {
                "const": "just",
                "type": "string"
              },
              "value": {
                "pattern": "^-?(?:0|[1-9]\\d*)$",
                "type": [
                  "integer",
                  "string"
                ],
                "format": "int32"
              }
            }
          }
        ]
      },
      "NullableFields": {
        "required": [
          "a",
          "b",
          "c"
        ],
        "type": "object",
        "properties": {
          "a": {
            "$ref": "#/components/schemas/Maybe"
          },
          "b": {
            "$ref": "#/components/schemas/Maybe"
          },
          "c": {
            "oneOf": [
              {
                "type": "null"
              },
              {
                "$ref": "#/components/schemas/Color"
              }
            ]
          }
        }
      },
      "NullableUnionFields": {
        "required": [
          "$type",
          "m",
          "n",
          "c"
        ],
        "type": "object",
        "properties": {
          "$type": {
            "const": "fields",
            "type": "string"
          },
          "m": {
            "$ref": "#/components/schemas/Maybe"
          },
          "n": {
            "oneOf": [
              {
                "type": "null"
              },
              {
                "$ref": "#/components/schemas/Color"
              }
            ]
          },
          "c": {
            "oneOf": [
              {
                "type": "null"
              },
              {
                "$ref": "#/components/schemas/Color"
              }
            ]
          }
        }
      },
      "Request8": {
        "required": [
          "record",
          "union"
        ],
        "type": "object",
        "properties": {
          "record": {
            "$ref": "#/components/schemas/NullableFields"
          },
          "union": {
            "$ref": "#/components/schemas/NullableUnionFields"
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

type Toggle =
    | Off
    | On of level: int

type InlineFields = {
    D: Color option
    T: Toggle option
    M: Maybe option
}
type InlineUnionFields = Inline of d: Color option * t: Toggle option * m: Maybe option
type Request9 = {
    Record: InlineFields
    Union: InlineUnionFields
}

[<Fact>]
let ``Inline optional enums and composite schemas are wrapped in a nullable oneOf`` () =
    task {
        let endpoints = [ POST [ route "/" <| text "Hello World" |> addOpenApiSimple<Request9, unit> ] ]
        use! server = WebApp.webAppInline endpoints
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
                "required": [
                  "record",
                  "union"
                ],
                "type": "object",
                "properties": {
                  "record": {
                    "required": [
                      "d",
                      "t",
                      "m"
                    ],
                    "type": "object",
                    "properties": {
                      "d": {
                        "oneOf": [
                          {
                            "type": "null"
                          },
                          {
                            "enum": [
                              "red",
                              "green",
                              "blue"
                            ],
                            "type": "string"
                          }
                        ]
                      },
                      "t": {
                        "oneOf": [
                          {
                            "type": "null"
                          },
                          {
                            "oneOf": [
                              {
                                "const": "off",
                                "type": "string"
                              },
                              {
                                "required": [
                                  "$type",
                                  "level"
                                ],
                                "type": "object",
                                "properties": {
                                  "$type": {
                                    "const": "on",
                                    "type": "string"
                                  },
                                  "level": {
                                    "pattern": "^-?(?:0|[1-9]\\d*)$",
                                    "type": [
                                      "integer",
                                      "string"
                                    ],
                                    "format": "int32"
                                  }
                                }
                              }
                            ]
                          }
                        ]
                      },
                      "m": {
                        "oneOf": [
                          {
                            "type": "null"
                          },
                          {
                            "required": [
                              "$type",
                              "value"
                            ],
                            "type": "object",
                            "properties": {
                              "$type": {
                                "const": "just",
                                "type": "string"
                              },
                              "value": {
                                "pattern": "^-?(?:0|[1-9]\\d*)$",
                                "type": [
                                  "integer",
                                  "string"
                                ],
                                "format": "int32"
                              }
                            }
                          }
                        ]
                      }
                    }
                  },
                  "union": {
                    "required": [
                      "$type",
                      "d",
                      "t",
                      "m"
                    ],
                    "type": "object",
                    "properties": {
                      "$type": {
                        "const": "inline",
                        "type": "string"
                      },
                      "d": {
                        "oneOf": [
                          {
                            "type": "null"
                          },
                          {
                            "enum": [
                              "red",
                              "green",
                              "blue"
                            ],
                            "type": "string"
                          }
                        ]
                      },
                      "t": {
                        "oneOf": [
                          {
                            "type": "null"
                          },
                          {
                            "oneOf": [
                              {
                                "const": "off",
                                "type": "string"
                              },
                              {
                                "required": [
                                  "$type",
                                  "level"
                                ],
                                "type": "object",
                                "properties": {
                                  "$type": {
                                    "const": "on",
                                    "type": "string"
                                  },
                                  "level": {
                                    "pattern": "^-?(?:0|[1-9]\\d*)$",
                                    "type": [
                                      "integer",
                                      "string"
                                    ],
                                    "format": "int32"
                                  }
                                }
                              }
                            ]
                          }
                        ]
                      },
                      "m": {
                        "oneOf": [
                          {
                            "type": "null"
                          },
                          {
                            "required": [
                              "$type",
                              "value"
                            ],
                            "type": "object",
                            "properties": {
                              "$type": {
                                "const": "just",
                                "type": "string"
                              },
                              "value": {
                                "pattern": "^-?(?:0|[1-9]\\d*)$",
                                "type": [
                                  "integer",
                                  "string"
                                ],
                                "format": "int32"
                              }
                            }
                          }
                        ]
                      }
                    }
                  }
                }
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
  "tags": [
    {
      "name": "Oxpecker.OpenApi.Tests"
    }
  ]
}"""
        resultString.ReplaceLineEndings() |> shouldEqual expected
    }
