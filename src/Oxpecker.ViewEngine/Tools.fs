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

module HTMLEncoding =
    let unicodeReplacementChar = 65533
    let HIGH_SURROGATE_START = int '\ud800'
    let LOW_SURROGATE_START = int '\uDC00'
    let UnicodeReplacementChar = '\uFFFD'

    let haveUnencodedChars (str: string) =
        let rec loop n =
            if n >= str.Length then
                -1
            else
                let mutable ch = str.[n]
                if ch <= '>' then
                    if '<' = ch then n
                    elif '>' = ch || '"' = ch || '\'' = ch || '&' = ch then n
                    else loop(n + 1)
                elif ch >= char 160 && ch < char 256 then
                    n
                elif Char.IsSurrogate ch then
                    n
                else
                    loop(n + 1)

        loop 0 <> -1

    let GetNextUnicodeScalarValueFromUtf16Surrogate (string: string) (index: int) : int =
        let mutable slice = string.AsSpan().Slice(index, 2)
        let mutable left = slice.[0]
        let mutable right = slice.[1]

        if string.Length - index <= 1 || not(Char.IsSurrogatePair(left, right)) then
            unicodeReplacementChar
        else
            (((int left - HIGH_SURROGATE_START) * 0x400)
             + (int right - LOW_SURROGATE_START)
             + 65536)


    let encodeCharsInto (sb: StringBuilder) (string: string) =
        let rec loop i =
            if i >= string.Length then
                ()
            else
                let mutable ch = string.[i]
                if ch <= '>' then
                    if '<' = ch then sb.Append "&lt;"
                    elif '>' = ch then sb.Append "&gt;"
                    elif '"' = ch then sb.Append "&quot;"
                    elif '\'' = ch then sb.Append "&#39;"
                    elif '&' = ch then sb.Append "&amp;"
                    else sb.Append ch
                    |> ignore
                    loop(i + 1)
                else
                    let mutable valueToEncode = -1
                    let mutable incr = 1

                    if ch >= char 160 && ch < char 256 then
                        valueToEncode <- int ch
                    else if Char.IsSurrogate ch then
                        let mutable scalarValue = GetNextUnicodeScalarValueFromUtf16Surrogate string i
                        incr <- incr + 1

                        if scalarValue >= 65536 then
                            valueToEncode <- scalarValue
                        else
                            ch <- char scalarValue

                    if valueToEncode >= 0 then
                        sb.Append("&#").Append(valueToEncode).Append ';'
                    else
                        sb.Append ch
                    |> ignore

                    loop(i + incr)

        if haveUnencodedChars string then
            loop 0

        else sb.Append string |> ignore

