module Oxpecker.Solid.Tests.Cases.Parameters

open Oxpecker.Solid
open Oxpecker.Solid.Aria

[<SolidComponent>]
let Test (id: int) (hello: string) =
    let helloWorld = hello + " world"
    div(id=string id, class'="testclass", ariaLabelledBy="testlabel") {
        if id > 0 then h3() else h2(class'=string id) { id }
        if id > 5 then h4(class'=helloWorld) else helloWorld
        match id with
        | 2 -> h1(id="two") { helloWorld }
        | x when x > 2 -> h1(id=string x)
        | _ -> h1()
    }
