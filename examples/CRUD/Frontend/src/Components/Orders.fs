module Frontend.Components.Orders

open Frontend.Components.CreateOrderButton
open Browser.Types
open Frontend.Shared.Data
open Oxpecker.Solid
open Shared

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
let Orders() =
    let mapItems (order: Order) =
        order.Items
        |> Array.map (fun item ->
            let product = products.latest |> Array.find (fun p -> p.ProductId = item.ProductId)
            { ProductName = product.Name; Amount = item.Amount }
            )

    article(class'="container mx-auto mt-10") {
        Show(when'=not orders.loading, fallback= p(class'="text-center text-2xl") { "Loading..." }) {
            div(class'="md:grid-cols-2 lg:grid-cols-3 gap-4 grid") {
                For(each=orders.latest) {
                    yield fun order _ ->
                        let showDetails, setShowDetails = createSignal false
                        let colWidth, setColWidth = createSignal "col-span-1"
                        let items, setItems = createSignal (mapItems order)

                        let onClick _ =
                            if not <| showDetails() then
                                setShowDetails true
                                setColWidth "col-span-2"
                                if items().Length = 0 then
                                    promise {
                                        let! orderDetails = fetchOrderDetails(order.OrderId)
                                        mapItems orderDetails
                                        |> setItems
                                    } |> ignore
                            else
                                setShowDetails false
                                setColWidth "col-span-1"

                        let delete (e: MouseEvent) =
                            e.stopPropagation()
                            promise {
                                let! _ = deleteOrder order.OrderId
                                ordersMgr.mutate(orders.latest |> Array.filter (fun o -> o.OrderId <> order.OrderId)) |> ignore
                            } |> ignore

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
                                button(class'="bg-red-500 text-white w-5 h-5 leading-4 p-0", onClick=delete) {
                                    "X"
                                }
                            }
                        }
                }
                CreateOrderButton()
            }
        }
    }
