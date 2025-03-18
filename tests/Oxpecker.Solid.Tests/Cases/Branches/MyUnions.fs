module Oxpecker.Solid.Tests.Cases.MyUnions

open Oxpecker.Solid

type StepType =
    | A
    | B of Accessor<int>

type Step =
    | One
    | Two of Accessor<StepType>
