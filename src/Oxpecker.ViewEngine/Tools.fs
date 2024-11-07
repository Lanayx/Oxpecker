module Oxpecker.ViewEngine.Tools

open System
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


/// <summary>
/// Lighter version of WebUtility.HtmlEncode made for StringBuilder
/// Implemented as per RFC
/// https://datatracker.ietf.org/doc/html/rfc1866#section-3.2.1
/// </summary>
/// <param name="sb">StringBuilder to write encoded chars to</param>
/// <param name="string">String to write to StringBuilder</param>
let HtmlEncode (sb: StringBuilder) (string: string) =
    let string = string.AsSpan()
    for i = 0 to string.Length - 1 do
        let ch = string.[i]
        if ch <= '>' then
            if '<' = ch then
                sb.Append "&lt;" |> ignore
            elif '>' = ch then
                sb.Append "&gt;" |> ignore
            elif '"' = ch then
                sb.Append "&quot;" |> ignore
            elif '\'' = ch then
                sb.Append "&#39;" |> ignore
            elif '&' = ch then
                sb.Append "&amp;" |> ignore
            else
                sb.Append ch |> ignore
        else
            sb.Append ch |> ignore
