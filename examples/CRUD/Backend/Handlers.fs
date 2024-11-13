module Backend.Handlers

open System
open System.Threading.Tasks
open Backend.Database
open Backend.Env
open Shared
open Backend.Services
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Oxpecker
open FSharp.UMX
open type Microsoft.AspNetCore.Http.TypedResults

type OperationEnv(env: Env) =
    interface IGetOrders with
        member this.GetOrders() = OrderRepository.getOrders env
    interface IGetOrder with
        member this.GetOrder id = OrderRepository.getOrder env id
    interface IGetProduct with
        member this.GetProduct id = ProductRepository.getProduct env id
    interface ICreateOrder with
        member this.CreateOrder order = OrderRepository.createOrder env order
    interface IUpdateOrder with
        member this.UpdateOrder order = OrderRepository.updateOrder env order
    interface IDeleteOrder with
        member this.DeleteOrder id = OrderRepository.deleteOrder env id
    interface IGetProducts with
        member this.GetProducts() = ProductRepository.getProducts env



let getOrders env (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! result = OrderService.getOrders operationEnv
        return! ctx.Write <| Ok result
    }
    :> Task

let getOrderDetails env id (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! result = OrderService.getOrder operationEnv id
        match result with
        | Ok order -> return! ctx.Write <| Ok order
        | Error error -> return! ctx.Write <| NotFound {| Error = error |}
    }
    :> Task

let createOrder env (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! newOrder = ctx.BindJson<NewOrder>()
        let! result = OrderService.createOrder operationEnv newOrder
        match result with
        | Ok orderId -> return! ctx.Write <| Ok {| OrderId = orderId |}
        | Error error ->
            env.Logger.LogError(error)
            return! ctx.Write <| BadRequest {| Error = error |}
    }
    :> Task

let updateOrder env (id: Guid) (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! order = ctx.BindJson<Order>()
        let! result = OrderService.updateOrder operationEnv order
        match result with
        | Ok _ -> return! ctx.Write <| NoContent()
        | Error error ->
            env.Logger.LogError(error)
            return! ctx.Write <| BadRequest {| Error = error |}
    }
    :> Task

let deleteOrder env id (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! result = OrderService.deleteOrder operationEnv id
        match result with
        | Ok _ -> return! ctx.Write <| NoContent()
        | Error error ->
            env.Logger.LogError(error)
            return! ctx.Write <| NotFound {| Error = error |}
    }
    :> Task

let getProducts env (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! result = ProductService.getProducts operationEnv
        return! ctx.Write <| Ok result
    }
    :> Task
