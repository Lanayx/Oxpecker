namespace Backend.Services

open System
open System.Threading.Tasks
open Shared
open FSharp.UMX

type IGetOrders =
    abstract member GetOrders: unit -> Task<Order[]>
type IGetOrder =
    abstract member GetOrder: Id -> Task<Order option>
type IGetProduct =
    abstract member GetProduct: Id -> Task<Product option>
type ICreateOrder =
    abstract member CreateOrder: Order -> Task<unit>
type IUpdateOrder =
    abstract member UpdateOrder: Order -> Task<unit>
type IDeleteOrder =
    abstract member DeleteOrder: Id -> Task<unit>
type IGetProducts =
    abstract member GetProducts: unit -> Task<Product[]>

module OrderService =
    let getOrders (env: #IGetOrders) = env.GetOrders()

    let getOrder (env: #IGetOrder) id =
        task {
            match! env.GetOrder id with
            | Some o ->
                return Ok o
            | None -> return Error "No order found"
        }

    let private checkOrderItems (env: #IGetProduct) (items: OrderItem[]) =
        items
        |> Array.map (fun (item: OrderItem) ->
            task {
                let! product = env.GetProduct item.ProductId
                match product with
                | Some p ->
                    if p.Quantity < item.Amount then
                        return Error $"Not enough {p.Name} in stock"
                    else
                        return Ok ()
                | _ -> return Error "No product found"
            })
        |> Task.WhenAll

    let createOrder (env: #ICreateOrder & #IGetProduct) (order: NewOrder) =
        task {
            let! itemsChecks = checkOrderItems env order.Items
            let errors = itemsChecks |> Array.choose (function Ok _ -> None | Error e -> Some e)
            if Array.isEmpty errors then
                let newOrder = {
                    OrderId = % Guid.NewGuid()
                    Description = order.Description
                    Items = order.Items
                    CreatedAt = DateTime.UtcNow
                }
                let! _ = env.CreateOrder newOrder
                return Ok newOrder.OrderId
            else
                return Error (String.concat ", " errors)
        }

    let updateOrder (env: #IUpdateOrder & #IGetOrder & #IGetProduct) (order: Order) =
        task {
            let! existingOrder = env.GetOrder order.OrderId
            match existingOrder with
            | Some _ ->
                let! itemsChecks = checkOrderItems env order.Items
                let errors = itemsChecks |> Array.choose (function Ok _ -> None | Error e -> Some e)
                if Array.isEmpty errors then
                    let! result = env.UpdateOrder order
                    return Ok result
                else
                    return Error (String.concat ", " errors)
            | _ -> return Error "No order found"
        }

    let deleteOrder (env: #IDeleteOrder & #IGetOrder) id =
        task {
            let! existingOrder = env.GetOrder id
            match existingOrder with
            | Some _ ->
                let! result = env.DeleteOrder id
                return Ok result
            | _ -> return Error "No order found"
        }

module ProductService =
    let getProducts (env: #IGetProducts) = env.GetProducts()
