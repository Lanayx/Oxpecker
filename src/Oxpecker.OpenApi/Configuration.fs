namespace Oxpecker.OpenApi

open System
open System.Reflection
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Metadata
open Microsoft.AspNetCore.OpenApi
open Microsoft.OpenApi

[<AutoOpen>]
module Configuration =

    // This is a hack to prevent generating Func tag in open API
    [<CompilerGenerated>]
    type internal FakeFunc =
        member this.InvokeUnit() = ()

    let internal fakeFuncMethod =
        typeof<FakeFunc>.GetMethod("InvokeUnit", BindingFlags.Instance ||| BindingFlags.NonPublic)
        |> nullArgCheck $"Method InvokeUnit not found"
    let internal unitType = typeof<unit>

    type RequestBody(?requestType: Type, ?contentTypes: string array, ?isOptional: bool) =
        let requestType = requestType |> Option.toObj
        let contentTypes = contentTypes |> Option.defaultValue [| "application/json" |]
        let isOptional = isOptional |> Option.defaultValue false
        member this.ToAttribute() =
            AcceptsMetadata(contentTypes, requestType, isOptional)

    type ResponseBody(?responseType: Type, ?contentTypes: string array, ?statusCode: int) =
        let responseType = responseType |> Option.toObj
        let contentTypes = contentTypes |> Option.toObj
        let statusCode = statusCode |> Option.defaultValue 200
        member this.ToAttribute() =
            ProducesResponseTypeMetadata(statusCode, responseType, contentTypes)

    type OpenApiConfig
        (
            ?requestBody: RequestBody,
            ?responseBodies: ResponseBody seq,
            ?configureOperation: OpenApiOperation -> OpenApiOperationTransformerContext -> CancellationToken -> Task
        ) =

        member this.Build(builder: IEndpointConventionBuilder) =
            builder.WithMetadata(fakeFuncMethod) |> ignore
            requestBody
            |> Option.iter(fun accepts -> builder.WithMetadata(accepts.ToAttribute()) |> ignore)
            responseBodies
            |> Option.iter(fun responseInfos ->
                for produces in responseInfos do
                    builder.WithMetadata(produces.ToAttribute()) |> ignore)
            match configureOperation with
            | Some configure -> builder.AddOpenApiOperationTransformer(configure)
            | None -> builder
