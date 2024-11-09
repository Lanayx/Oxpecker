module Oxpecker.Solid.Tests.Cases.CustomTags

open System
open Oxpecker.Solid
open Oxpecker.Solid.Tests.Cases.MyTag

[<SolidComponent>]
let Test () =
    MyTag(myAttr = "foo") {
        "Hello " + DateTime().Day.ToString()
        MyTag()
    }
