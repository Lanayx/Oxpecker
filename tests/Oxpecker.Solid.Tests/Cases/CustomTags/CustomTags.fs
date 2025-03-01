module Oxpecker.Solid.Tests.Cases.CustomTags

open System
open Oxpecker.Solid
open Oxpecker.Solid.Tests.Cases.MyTag
open Oxpecker.Solid.Tests.Cases.ImportedTag

[<SolidComponent>]
let Test2 () =
    ImportedTag().attr("x", "y").attr("a", "b") {
        ImportedTag().attr("x", "y").attr("a", "b")
        "Hello"
    }

[<SolidComponent>]
let Test1 () =
    MyTag(myAttr = "foo") {
        "Hello " + DateTime().Day.ToString()
        MyTag()
    }
