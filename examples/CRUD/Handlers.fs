module CRUD.Handlers

open System
open System.Threading.Tasks
open CRUD.Database
open CRUD.Env
open CRUD.Models
open CRUD.Services
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Oxpecker
open FSharp.UMX

type OperationEnv(env: Env) =
    interface IGetOrders with
        member this.GetOrders () = OrderRepository.getOrders env
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

type OrderDTO =
    {
        ProductId: Guid
        Amount: uint
        Description: string
    }


let getOrders env (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! result = OrderService.getOrders operationEnv
        return! ctx.WriteJson(result)
    } :> Task

let getOrderDetails env id (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! result = OrderService.getOrder operationEnv id
        match result with
        | Ok order ->
            return! ctx.WriteJson(order)
        | Error error ->
            env.Logger.LogError(error)
            ctx.SetStatusCode(StatusCodes.Status404NotFound)
            return! ctx.WriteText(error)
    } :> Task

let createOrder env (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! dto = ctx.BindJson<OrderDTO>()
        let order = {
            OrderId = %Guid.NewGuid()
            ProductId = %dto.ProductId
            Amount = dto.Amount
            CreatedAt = DateTime.UtcNow
            Description = dto.Description
        }
        let! result = OrderService.createOrder operationEnv order
        match result with
        | Ok _ ->
            ctx.SetStatusCode(StatusCodes.Status201Created)
        | Error error ->
            env.Logger.LogError(error)
            ctx.SetStatusCode(StatusCodes.Status400BadRequest)
            return! ctx.WriteText(error)
    } :> Task

let updateOrder env (id: Guid) (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! dto = ctx.BindJson<OrderDTO>()
        let order = {
            OrderId = %id
            ProductId = %dto.ProductId
            Amount = dto.Amount
            CreatedAt = DateTime.UtcNow
            Description = dto.Description
        }
        let! result = OrderService.updateOrder operationEnv order
        match result with
        | Ok _ ->
            ctx.SetStatusCode(StatusCodes.Status204NoContent)
        | Error error ->
            env.Logger.LogError(error)
            ctx.SetStatusCode(StatusCodes.Status400BadRequest)
            return! ctx.WriteText(error)

    } :> Task

let deleteOrder env id (ctx: HttpContext) =
    task {
        let operationEnv = OperationEnv(env)
        let! result = OrderService.deleteOrder operationEnv id
        match result with
        | Ok _ ->
            ctx.SetStatusCode(StatusCodes.Status204NoContent)
        | Error error ->
            env.Logger.LogError(error)
            ctx.SetStatusCode(StatusCodes.Status404NotFound)
            return! ctx.WriteText(error)
    } :> Task