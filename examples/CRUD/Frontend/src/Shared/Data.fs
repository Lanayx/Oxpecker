module Frontend.Shared.Data

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

let createOrder (newOrder: NewOrder) : JS.Promise<{| OrderId: Id |}> =
  promise {
      let! response = Fetch.post($"{apiBase}/order", newOrder, caseStrategy = CaseStrategy.CamelCase)
      return response
  }

let deleteOrder (id: Id) : JS.Promise<unit> =
  promise {
      let! response = Fetch.delete($"{apiBase}/order/{id}")
      return response
  }

let orders, ordersMgr = createResource(fetchOrders)
let products, _ = createResource(fetchProducts)
