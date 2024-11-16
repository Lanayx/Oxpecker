module Oxpecker.ViewEngine.Tools

open System
open System.Globalization
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

module CustomWebUtility =

    // Constants
    [<Literal>]
    let private HIGH_SURROGATE_START = '\uD800'
    [<Literal>]
    let private LOW_SURROGATE_START = '\uDC00'
    [<Literal>]
    let private LOW_SURROGATE_END = '\uDFFF'
    [<Literal>]
    let private UNICODE_PLANE00_END = 0x00FFFF
    [<Literal>]
    let private UNICODE_PLANE01_START = 0x10000
    [<Literal>]
    let private UNICODE_PLANE16_END = 0x10FFFF

    [<Literal>]
    let private UnicodeReplacementChar = '\uFFFD'
    [<Literal>]
    let private MaxInt32Digits = 10

    // Helper function to get the next Unicode scalar value from UTF-16 surrogate pairs
    let private getNextUnicodeScalarValueFromUtf16Surrogate (input: ReadOnlySpan<char>) (i: byref<int>) : int =
        let highSurrogate = input[i]
        if i + 1 < input.Length then
            let lowSurrogate = input[i + 1]
            if Char.IsLowSurrogate(lowSurrogate) then
                i <- i + 1 // Advance the index as we've consumed two chars
                Char.ConvertToUtf32(highSurrogate, lowSurrogate)
            else
                int UnicodeReplacementChar
        else
            int UnicodeReplacementChar

    // Function to encode HTML entities
    let private htmlEncodeInner (input: ReadOnlySpan<char>) (sb: StringBuilder) : unit =
        let mutable i = 0
        while i < input.Length do
            let ch = input[i]
            if ch <= '>' then
                if '<' = ch then sb.Append "&lt;"
                elif '>' = ch then sb.Append "&gt;"
                elif '"' = ch then sb.Append "&quot;"
                elif '\'' = ch then sb.Append "&#39;"
                elif '&' = ch then sb.Append "&amp;"
                else sb.Append ch
                |> ignore
            else
                let mutable valueToEncode = -1 // Set to >= 0 if needs to be encoded
                if ch >= '\u00A0' && ch < '\u0100' then
                    valueToEncode <- int ch
                elif Char.IsSurrogate(ch) then
                    let mutable scalarValue = getNextUnicodeScalarValueFromUtf16Surrogate input &i
                    if scalarValue >= UNICODE_PLANE01_START then
                        valueToEncode <- scalarValue
                    else
                        // Don't encode BMP characters (like U+FFFD)
                        sb.Append(char scalarValue) |> ignore
                if valueToEncode >= 0 then
                    sb.Append("&#")
                      .Append(valueToEncode)
                      .Append(';') |> ignore
                else
                    sb.Append(ch) |> ignore
            i <- i + 1

    // Function to find the index of characters that need HTML encoding
    let internal indexOfHtmlEncodingChars (input: string) : int =
        let rec loop i =
            if i < input.Length then
                let ch = input[i]
                if ch <= '>' then
                    if ch = '<' || ch = '>' || ch = '"' || ch = '\'' || ch = '&' then
                        i
                    else
                        loop (i + 1)
                elif ch >= '\u00A0' && ch < '\u0100' then
                    i
                elif Char.IsSurrogate(ch) then
                    i
                else
                    loop (i + 1)
            else
                -1
        loop 0


    let htmlEncode (value: string) (sb: StringBuilder) =
        if isNull value then
            sb.Append(value) |> ignore
        else
            match indexOfHtmlEncodingChars value with
            | -1 ->
                sb.Append(value) |> ignore
            | index ->
                let value = value.AsSpan()
                sb.Append(value.Slice(0, index)) |> ignore
                htmlEncodeInner (value.Slice(index)) sb
