namespace CRUD.Services

open System.Threading.Tasks
open CRUD.Models

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

module OrderService =
    let getOrders (env: #IGetOrders) = env.GetOrders()

    let getOrder (env: #IGetOrder & #IGetProduct) id =
        task {
            match! env.GetOrder id with
            | Some o ->
                let! product = env.GetProduct o.ProductId
                match product with
                | Some p ->
                    return
                        Ok {
                            OrderDetails.OrderId = o.OrderId
                            Amount = o.Amount
                            Description = o.Description
                            CreatedAt = o.CreatedAt
                            ProductName = p.Name
                        }
                | _ -> return Error "No product found"
            | None -> return Error "No order found"
        }

    let createOrder (env: #ICreateOrder & #IGetProduct) (order: Order) =
        task {
            let! product = env.GetProduct order.ProductId
            match product with
            | Some p ->
                if p.Quantity < order.Amount then
                    return Error "Not enough products in stock"
                else
                    let! result = env.CreateOrder order
                    return Ok result
            | _ -> return Error "No product found"
        }

    let updateOrder (env: #IUpdateOrder & #IGetOrder & #IGetProduct) (order: Order) =
        task {
            let! existingOrder = env.GetOrder order.OrderId
            let! product = env.GetProduct order.ProductId
            match existingOrder, product with
            | Some _, Some p ->
                if p.Quantity < order.Amount then
                    return Error "Not enough products in stock"
                else
                    let! result = env.UpdateOrder order
                    return Ok result
            | _ -> return Error "No order or product found"
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
