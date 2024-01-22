module CRUD.Models

open System
open System.Threading.Tasks
open FSharp.UMX
open Microsoft.Extensions.Logging

[<Measure>]
type private id
type Id = Guid<id>

type Order = {
    OrderId: Id
    ProductId: Id
    Amount: uint
    Description: string
    CreatedAt: DateTime
}

type OrderDetails = {
    OrderId: Id
    ProductName: string
    Amount: uint
    Description: string
    CreatedAt: DateTime
}

type Product = {
    ProductId: Id
    Name: string
    Quantity: uint
}


// abstractions
type IDbClient =
    abstract member ExecuteStatement: sql: string -> Task<'T>

type IDbEnv =
    abstract DbClient: IDbClient

type IAppLogger =
    abstract Logger: ILogger
