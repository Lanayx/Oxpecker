module Oxpecker.Solid.Tests.Cases.MyTag

open Oxpecker.Solid

type MyTag () =
    inherit RegularNode()
    member this.myAttr
        with set (value: string) = ()
