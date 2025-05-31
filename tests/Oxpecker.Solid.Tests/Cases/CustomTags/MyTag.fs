module Oxpecker.Solid.Tests.Cases.MyTag

open Oxpecker.Solid

[<CompiledName("my-tag")>]
type MyTag () =
    interface RegularNode
    member this.myAttr
        with set (value: string) = ()
