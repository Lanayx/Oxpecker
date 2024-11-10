module App

open Oxpecker.Solid
open Fable.Core
open Shared
open Thoth.Fetch
open Thoth.Json

let apiBase = "http://localhost:5000"

type DisplayItem = {
    ProductName: string
    Amount: uint
}

let fetchOrders() : JS.Promise<Order[]> =
  promise {
      let! response = Fetch.get($"{apiBase}/order", caseStrategy = CaseStrategy.CamelCase)
      return response
  }

let fetchOrderDetails id : JS.Promise<Order> =
  promise {
      let! response = Fetch.get($"{apiBase}/order/{id}", caseStrategy = CaseStrategy.CamelCase)
      return response
  }

let fetchProducts() : JS.Promise<Product[]> =
  promise {
      let! response = Fetch.get($"{apiBase}/product", caseStrategy = CaseStrategy.CamelCase)
      return response
  }

let orders, _ = createResource(fetchOrders)
let products, _ = createResource(fetchProducts)

[<SolidComponent>]
let OrderItems (items: DisplayItem[]) =
    div() {
        For(each=items) {
            yield fun item _ ->
                div(class'="flex") {
                    p() {
                        span(class'="font-bold mr-4") { item.ProductName }
                    }
                    p() {
                        span() { $"{item.Amount}" }
                    }
                }
        }
    }

[<SolidComponent>]
let CreateOrderButton() =
    div(class'="bg-gray-100 shadow-md cursor-pointer p-4") {
        h2(class'="text-xl font-bold") {
            "Create Order"
        }
        p() {
            "Create a new order"
        }
    }

[<SolidComponent>]
let Orders() =

    article(class'="container mx-auto mt-10") {
        Show(when'=not orders.loading, fallback= p(class'="text-center text-2xl") { "Loading..." }) {
            div(class'="md:grid-cols-2 lg:grid-cols-3 gap-4 grid") {
                For(each=orders.latest) {
                    yield fun order _ ->
                        let showDetails, setShowDetails = createSignal false
                        let colWidth, setColWidth = createSignal "col-span-1"
                        let items, setItems = createSignal [||]
                        let onClick _ =
                            if not <| showDetails() then
                                setShowDetails true
                                setColWidth "col-span-2"
                                if items().Length = 0 then
                                    promise {
                                        let! orderDetails = fetchOrderDetails(order.OrderId)
                                        orderDetails.Items
                                        |> Array.map (fun item ->
                                            let product = products.latest |> Array.find (fun p -> p.ProductId = item.ProductId)
                                            { ProductName = product.Name; Amount = item.Amount }
                                            )
                                        |> setItems
                                    } |> ignore
                            else
                                setShowDetails false
                                setColWidth "col-span-1"

                        div(class'="bg-white shadow-md cursor-pointer p-4 " + colWidth(), onClick= onClick) {
                            div(class'="flex justify-between") {
                                div(class'="flex-1") {
                                    h2(class'="text-xl font-bold") {
                                        order.Description
                                    }
                                    p() {
                                        order.CreatedAt.ToShortTimeString()
                                    }
                                }
                                Show(when'=showDetails()){
                                    div(class'="flex-1") {
                                        OrderItems(items())
                                    }
                                }
                            }
                        }
                }
                CreateOrderButton()
            }
        }
    }


[<SolidComponent>]
let App() =
    main(class'="container mx-auto") {
        h1(class'="text-4xl font-bold text-center mt-10") {
            "CRUD App"
        }
        Orders()
    }
