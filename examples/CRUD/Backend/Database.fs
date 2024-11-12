namespace Backend.Database

open System
open System.Threading.Tasks
open Shared
open FSharp.UMX
open Microsoft.Extensions.Logging
open Backend.Abstractions

module DB =
    let executeStatement (env: #IDbEnv & #IAppLogger) sql =
        env.Logger.LogDebug(sql)
        env.DbClient.ExecuteStatement<'T> sql
    let executeTransaction (env: #IDbEnv & #IAppLogger) sqlStatements =
        for sql in sqlStatements do
            env.Logger.LogDebug(sql)
        env.DbClient.ExecuteTransaction sqlStatements

module OrderRepository =

    type DbOrderItem = {
        ProductId: Guid
        Amount: uint
    }

    type DbOrder = {
        OrderId: Guid
        Description: string
        CreatedAt: DateTime
    } with
        member this.ToDomain() = {
            Order.OrderId = %this.OrderId
            Description = this.Description
            CreatedAt = this.CreatedAt
            Items = [||]
        }
        member this.ToDomain(items: DbOrderItem[]) = {
            Order.OrderId = %this.OrderId
            Description = this.Description
            CreatedAt = this.CreatedAt
            Items = items |> Array.map (fun item -> { ProductId = %item.ProductId; Amount = item.Amount })
        }

    type Order with
        member this.ToDatabase() =
            {|
                DbOrder = {
                    DbOrder.OrderId = %this.OrderId
                    Description =  this.Description
                    CreatedAt = this.CreatedAt
                }
                DbItems = this.Items |> Array.map (fun item -> { ProductId = %item.ProductId; Amount = item.Amount })
            |}

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
            let itemsSql = $"SELECT * FROM OrderItems WHERE OrderId = {id}"
            let! (dbItems: DbOrderItem[]) = DB.executeStatement env itemsSql
            return dbOrder |> Option.map _.ToDomain(dbItems)
        }

    let createOrder env (order: Order) =
        task {
            let db = order.ToDatabase()
            let sqlStatements = [|
                yield $"INSERT INTO Orders VALUES ({db.DbOrder.OrderId}, {db.DbOrder.Description}, {db.DbOrder.CreatedAt})"
                for item in db.DbItems do
                    yield $"INSERT INTO OrderItems VALUES ({db.DbOrder.OrderId}, {item.ProductId}, {item.Amount})"
            |]

            let! (result: unit) = DB.executeTransaction env sqlStatements
            return result
        }
    let updateOrder env (order: Order) =
        task {
            let dbOrder = order.ToDatabase()
            let sqlStatements = [|
                yield $"UPDATE Orders SET Description = {dbOrder.DbOrder.Description}, CreatedAt = {dbOrder.DbOrder.CreatedAt} WHERE OrderId = {dbOrder.DbOrder.OrderId}"
                yield $"DELETE FROM OrderItems WHERE OrderId = {dbOrder.DbOrder.OrderId}"
                for item in dbOrder.DbItems do
                    yield $"INSERT INTO OrderItems VALUES ({dbOrder.DbOrder.OrderId}, {item.ProductId}, {item.Amount})"
            |]
            let! (result: unit) = DB.executeTransaction env sqlStatements
            return result
        }

    let deleteOrder env (id: Id) =
        task {
            let sqlStatements = [|
                $"DELETE FROM Orders WHERE ID = {id}"
                $"DELETE FROM OrderItems WHERE OrderId = {id}"
            |]
            let! (result: unit) = DB.executeTransaction env sqlStatements
            return result
        }

open OrderRepository

module ProductRepository =
    type DbProduct = {
        ProductId: Guid
        Quantity: uint
        Name: string
    } with
        member this.ToDomain() = {
            Product.ProductId = %this.ProductId
            Quantity = this.Quantity
            Name = this.Name
        }

    type Product with
        member this.ToDatabase() = {
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

    let getProducts env =
        task {
            let sql = "SELECT * FROM Products"
            let! (dbProducts: DbProduct[]) = DB.executeStatement env sql
            return dbProducts |> Array.map _.ToDomain()
        }

module Fake =

    open OrderRepository
    open ProductRepository

    let products = [|
        {
            DbProduct.ProductId = Guid.NewGuid()
            Quantity = 10u
            Name = "First product"
        }
        {
            DbProduct.ProductId = Guid.NewGuid()
            Quantity = 20u
            Name = "Second product"
        }
    |]

    let orders = [|
        {
            DbOrder.OrderId = Guid.NewGuid()
            Description = "First order"
            CreatedAt = DateTime.UtcNow.AddMinutes(-2.)
        }
        {
            DbOrder.OrderId = Guid.NewGuid()
            Description = "Second order"
            CreatedAt = DateTime.UtcNow
        }
    |]

    let orderItems =
        [|
            (orders[0].OrderId, { ProductId = products[0].ProductId; Amount = 10u })
            (orders[0].OrderId, { ProductId = products[1].ProductId; Amount = 15u })
            (orders[1].OrderId, { ProductId = products[0].ProductId; Amount = 20u })
        |]

    let executeStatement<'T> (sql: string) =
        let t = typeof<'T>
        match t with
        | x when x = typeof<DbOrder[]> ->
            orders
            |> box
            :?> 'T
        | x when x = typeof<DbOrderItem[]> ->
            [|
                for key, value in orderItems do
                    if sql.Contains(key.ToString()) then
                        yield value
            |]
            |> box
            :?> 'T
        | x when x = typeof<DbOrder option> ->
            {
                DbOrder.OrderId = Guid.NewGuid()
                Description = "First order"
                CreatedAt = DateTime.UtcNow
            }
            |> Some
            |> box
            :?> 'T
        | x when x = typeof<DbProduct option> ->
            {
                DbProduct.ProductId = Guid.NewGuid()
                Name = "First product"
                Quantity = 20u
            }
            |> Some
            |> box
            :?> 'T
        | x when x = typeof<DbProduct[]> ->
            products |> box :?> 'T
        | _ ->
            () |> box :?> 'T
    let fakeClient =
        { new IDbClient with
            member this.ExecuteStatement<'T> sql =
                task {
                    do! Task.Delay(500)
                    return executeStatement<'T> sql
                }
            member this.ExecuteTransaction(sqlStatements) =
                task {
                    do! Task.Delay(500)
                    return ()
                }
        }
