module Oxpecker.Solid.Tests.Cases.MyTag

open Oxpecker.Solid

[<CompiledName("my-tag")>]
type MyTag () =
    inherit RegularNode()
    member this.myAttr
        with set (value: string) = ()
