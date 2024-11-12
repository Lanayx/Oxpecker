namespace Shared

open FSharp.UMX
open System

[<Measure>]
type private id
type Id = Guid<id>

type OrderItem = {
    ProductId: Id
    Amount: uint
}

type Order = {
    OrderId: Id
    Description: string
    Items: OrderItem[]
    CreatedAt: DateTime
}

type Product = {
    ProductId: Id
    Name: string
    Quantity: uint
}

type NewOrder = {
    Items: OrderItem[]
    Description: string
}
