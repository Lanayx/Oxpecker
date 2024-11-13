module Frontend.Components.CreateOrderButton

open System
open Frontend.Shared.Data
open Oxpecker.Solid
open Shared
open FSharp.UMX
open Fable.Core.JsInterop



[<SolidComponent>]
let CreateOrderButton() =
    let editMode, setEditMode = createSignal false
    let colWidth, setColWidth = createSignal "col-span-1"
    let items, setItems = createSignal [||]
    let firstProduct() =
        if isNull products.current then
            %Guid.Empty
        else
            products.current |> Array.tryHead |> Option.map _.ProductId |> Option.defaultValue %Guid.Empty
    let productName productId =
        if isNull products.current then
            ""
        else
            products.current |> Array.find (fun p -> p.ProductId = productId) |> _.Name
    let newProductId, setNewProductId = createSignal <| firstProduct()
    let newAmount, setNewAmount = createSignal 0u
    let newDescription, setNewDescription = createSignal ""

    let isDisabled() =
        let description = newDescription()
        items().Length = 0 || String.IsNullOrEmpty(description)

    let onCreateOrderClick _ =
        setColWidth "col-span-2"
        setEditMode true

    let onAddItem _ =
        items()
        |> Array.insertAt (items().Length) { ProductId = newProductId(); Amount = newAmount() }
        |> setItems
        setNewProductId <| firstProduct()
        setNewAmount 0u

    let close() =
        setColWidth "col-span-1"
        setItems [||]
        setNewDescription ""
        setEditMode false

    let onSave _ =
        promise {
            let! orderId = createOrder { Description = newDescription(); Items = items() }
            let newOrder = { OrderId = orderId.OrderId; Description = newDescription(); Items = items(); CreatedAt = DateTime.UtcNow }
            ordersMgr.mutate(orders.latest |> Array.insertAt orders.latest.Length newOrder ) |> ignore
            close()
        } |> ignore

    div(class'="bg-gray-100 shadow-md cursor-pointer p-4 " + colWidth()) {
        Switch() {
            Match(when'=editMode()){
                div(class'="flex") {
                    div(class'="flex-shrink") {
                        h2(class'="text-xl font-bold") {
                            "Description"
                        }
                        input(type'="text", class'="border p-2 w-80 mr-5", placeholder="My order",
                              onChange = fun e -> setNewDescription e.target?value)
                    }
                    div(class'="flex flex-col flex-grow") {
                        For(each=items()) {
                            yield fun item _ ->
                                div(class'="flex gap-8 mb-4") {
                                    span(class'="font-bold") { productName item.ProductId }
                                    span() { $"{item.Amount}" }
                                }
                        }
                        div(class'="flex justify-between w-full h-max gap-8") {
                            select(class'="border p-2 w-full", onChange=fun e ->
                                    let newProductId: string = e.target?value
                                    setNewProductId <| %Guid.Parse newProductId) {
                                For(each=products.latest) {
                                    yield fun product _ ->
                                        option(value=string product.ProductId,
                                               selected= (product.ProductId = newProductId())) { product.Name }
                                }
                            }
                            input(type'="number", class'="border p 2 w-28", placeholder="Amount",
                                  value=(newAmount() |> string),
                                  onChange=fun e -> setNewAmount <| uint e.target?value)
                            button(class'="bg-blue-500 text-white p-2", onClick=onAddItem ) {
                                "Add"
                            }
                        }
                    }
                }
                div(class'="flex flex-row-reverse mt-12 gap-8") {
                    button(class'="bg-red-500 text-white p-2 flex-1",
                           onClick=fun _ -> close()) {
                        "Cancel"
                    }
                    button(class'="bg-green-500 text-white p-2 flex-1 disabled:bg-gray-300 ", disabled = isDisabled(), onClick = onSave) {
                        "Save"
                    }
                    div(class'="flex-1")
                    div(class'="flex-1")
                }
            }
            Match(when'= (editMode() |> not)){
                div(onClick=onCreateOrderClick) {
                    h2(class'="text-xl font-bold") {
                        "Create Order"
                    }
                    p() {
                        "Create a new order"
                    }
                }
            }
        }
    }

