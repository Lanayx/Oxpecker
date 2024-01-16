namespace CRUD.Database

open System
open System.Threading.Tasks
open CRUD.Models
open FSharp.UMX
open Microsoft.Extensions.Logging

module DB =
    let executeStatement (env: #IDbEnv & #IAppLogger) sql =
        env.Logger.LogDebug(sql)
        env.DbClient.ExecuteStatement<'T> sql

module OrderRepository =

    type DbOrder = {
        OrderId: Guid
        ProductId: Guid
        Amount: uint
        Description: string
        CreatedAt: DateTime
    }
    with
        member this.ToDomain() =
            {
                Order.OrderId = %this.OrderId
                ProductId = %this.ProductId
                Amount = this.Amount
                Description = this.Description
                CreatedAt = this.CreatedAt
            }

    type Order with
        member this.ToDatabase() =
            {
                OrderId = %this.OrderId
                ProductId = %this.ProductId
                Amount = this.Amount
                Description = this.Description
                CreatedAt = this.CreatedAt
            }

    type DbOrderDetails = {
        OrderId: Guid
        ProductName: string
        Amount: uint
        Description: string
        CreatedAt: DateTime
    }
    with
        member this.ToDomain() =
            {
                OrderDetails.OrderId = %this.OrderId
                ProductName = this.ProductName
                Amount = this.Amount
                Description = this.Description
                CreatedAt = this.CreatedAt
            }

    type OrderDetails with
        member this.ToDatabase() =
            {
                OrderId = %this.OrderId
                ProductName = %this.ProductName
                Amount = this.Amount
                Description = this.Description
                CreatedAt = this.CreatedAt
            }

    let getOrders env =
        task {
            let sql = "SELECT * FROM Orders"
            let! (dbOrders: DbOrder[]) = DB.executeStatement env sql
            return dbOrders |> Array.map _.ToDomain()
        }

    let getOrder env (id: Id) =
        task {
            let sql = $"SELECT * FROM Orders WHERE ID = {id}"
            let! (dbOrder: DbOrder option) = DB.executeStatement env sql
            return dbOrder |> Option.map _.ToDomain()
        }

    let createOrder env (order: Order) =
        task {
            let dbOrder = order.ToDatabase()
            let sql = $"INSERT INTO Orders VALUES ({dbOrder.OrderId}, {dbOrder.ProductId}, {dbOrder.Amount}, {dbOrder.Description}, {dbOrder.CreatedAt})"
            let! (result: unit) = DB.executeStatement env sql
            return result
        }
    let updateOrder env (order: Order) =
        task {
            let dbOrder = order.ToDatabase()
            let sql = $"UPDATE Orders SET ProductId = {dbOrder.ProductId}, Amount = {dbOrder.Amount}, Description = {dbOrder.Description}, CreatedAt = {dbOrder.CreatedAt} WHERE OrderId = {dbOrder.OrderId}"
            let! (result: unit) = DB.executeStatement env sql
            return result
        }

    let deleteOrder env (id: Id) =
        task {
            let sql = $"DELETE FROM Orders WHERE OrderId = {id}"
            let! (result: unit) = DB.executeStatement env sql
            return result
        }

module ProductRepository =
    type DbProduct = {
        ProductId: Guid
        Quantity: uint
        Name: string
    }
    with
        member this.ToDomain() =
            {
                Product.ProductId = %this.ProductId
                Quantity = this.Quantity
                Name = this.Name
            }

    type Product with
        member this.ToDatabase() =
            {
                ProductId = %this.ProductId
                Quantity = this.Quantity
                Name = this.Name
            }

    let getProduct env (id: Id) =
        task {
            let sql = $"SELECT * FROM Products WHERE ID = {id}"
            let! (dbProduct: DbProduct option) = DB.executeStatement env sql
            return dbProduct |> Option.map _.ToDomain()
        }

module Fake =

    open OrderRepository
    open ProductRepository

    let fakeClient = {
        new IDbClient with
            member this.ExecuteStatement<'T> sql =
                let t = typeof<'T>
                match t with
                | x when x = typeof<DbOrder array> ->
                    Task.FromResult [| {
                        DbOrder.OrderId = Guid.NewGuid()
                        ProductId = Guid.NewGuid()
                        Amount = 10u
                        Description = "First order"
                        CreatedAt = DateTime.UtcNow
                    } |] |> box :?> Task<'T>
                | x when x = typeof<DbOrder option> ->
                    {
                        DbOrder.OrderId = Guid.NewGuid()
                        ProductId = Guid.NewGuid()
                        Amount = 10u
                        Description = "First order"
                        CreatedAt = DateTime.UtcNow
                    } |> Some |> Task.FromResult |> box :?> Task<'T>
                | x when x = typeof<DbProduct option> ->
                    {
                        DbProduct.ProductId =  Guid.NewGuid()
                        Name = "First product"
                        Quantity = 20u
                    } |> Some |> Task.FromResult |> box :?> Task<'T>
                | _ ->
                    () |> Task.FromResult |> box :?> Task<'T>
    }