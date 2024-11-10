module Oxpecker.ViewEngine.Tools

open System
open System.Numerics
open System.Runtime.CompilerServices
open System.Text
open Microsoft.Extensions.ObjectPool

let StringBuilderPool = DefaultObjectPoolProvider().CreateStringBuilderPool()

/// <summary>
/// Checks if an object is not null.
/// </summary>
/// <param name="x">The object to validate against `null`.</param>
/// <returns>Returns true if the object is not null otherwise false.</returns>
let inline internal isNotNull x = not(isNull x)

[<AllowNullLiteral>]
type internal CustomQueueItem<'T>(value: 'T) =
    member this.Value = value
    member val Next = Unchecked.defaultof<CustomQueueItem<'T>> with get, set

[<Struct>]
type internal CustomQueue<'T> =
    val mutable Head: CustomQueueItem<'T>
    val mutable Tail: CustomQueueItem<'T>
    member this.Enqueue(value: 'T) =
        let item = CustomQueueItem(value)
        if isNull this.Head then
            this.Head <- item
            this.Tail <- item
        else
            this.Tail.Next <- item
            this.Tail <- item

    member this.AsEnumerable() =
        let mutable next = this.Head
        seq {
            while isNotNull next do
                yield next.Value
                next <- next.Next
        }

let unicodeReplacementChar: char = char 65533
let HIGH_SURROGATE_START = int '\ud800'
let LOW_SURROGATE_START = int '\uDC00'
let UnicodeReplacementChar = '\uFFFD'
let lt = int '>'


let inline haveUnencodedChars (s: string) =
    let mutable i = 0
    let mutable notTrue = false

    while not notTrue do
        let mutable c = s.[i]
        let ci: int = Unsafe.As &c
        let mutable b = lt > ci
        notTrue <- b
        i <- i + 1

    notTrue

let HtmlEncode (sb: StringBuilder) (string: string) =
    if string |> haveUnencodedChars then
        let string = string.AsSpan()

        let mutable i = 0
        while i < string.Length do
            let mutable ch = string.[i]
            if ch <= '>' then
                if '<' = ch then sb.Append "&lt;" |> ignore
                elif '>' = ch then sb.Append "&gt;" |> ignore
                elif '"' = ch then sb.Append "&quot;" |> ignore
                elif '\'' = ch then sb.Append "&#39;" |> ignore
                elif '&' = ch then sb.Append "&amp;" |> ignore
                else sb.Append ch |> ignore
                i <- i + 1
            else
                let mutable valueToEncode: int = -1
                if Char.IsSurrogate ch then
                    let mutable slice = string.Slice(i, 2)
                    let left = slice.[0]
                    let right = slice.[1]

                    if not(Char.IsSurrogatePair(left, right)) then
                        sb.Append UnicodeReplacementChar |> ignore

                    let mutable scalarValue: int =
                        (((int left - HIGH_SURROGATE_START) * 0x400)
                         + (int right - LOW_SURROGATE_START)
                         + 65536)

                    i <- i + 2

                    if scalarValue >= 65536 then
                        valueToEncode <- scalarValue
                    else
                        ch <- Unsafe.As &scalarValue

                if valueToEncode >= 0 then
                    sb.Append "&#" |> ignore
                    sb.Append valueToEncode |> ignore
                    sb.Append ';' |> ignore
                else
                    sb.Append ch |> ignore
                    i <- i + 1
    else
        sb.Append string |> ignore
