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

module CustomWebUtility =

    // Constants
    [<Literal>]
    let private UNICODE_PLANE01_START = 0x10000
    [<Literal>]
    let private UnicodeReplacementChar = 0xFFFD

    let private getNextUnicodeScalarValueFromUtf16Surrogate (input: ReadOnlySpan<char>) (index: byref<int>) : int =

        if input.Length - index <= 1 then
            // Not enough characters to form a surrogate pair
            UnicodeReplacementChar
        else
            let leadingSurrogate = input[index]
            let trailingSurrogate = input[index + 1]

            if
                leadingSurrogate >= '\uD800'
                && leadingSurrogate <= '\uDBFF'
                && trailingSurrogate >= '\uDC00'
                && trailingSurrogate <= '\uDFFF'
            then
                // Consume the trailing surrogate
                index <- index + 1

                // Calculate the Unicode scalar value
                ((int leadingSurrogate - 0xD800) <<< 10)
                + (int trailingSurrogate - 0xDC00)
                + 0x10000
            else
                // Unmatched surrogate
                UnicodeReplacementChar

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
                let mutable valueToEncode = -1
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
                    sb.Append("&#").Append(valueToEncode).Append(';') |> ignore
                else
                    sb.Append(ch) |> ignore

            i <- i + 1

    // Function to find the index of characters that need HTML encoding back
    let internal indexOfHtmlEncodingCharsBack (input: string) : int =
        let rec loop i =
            if i > 0 then
                let ch = input[i]
                if ch <= '>' then
                    if ch = '<' || ch = '>' || ch = '"' || ch = '\'' || ch = '&' then
                        i
                    else
                        loop(i - 1)
                elif ch >= '\u00A0' && ch < '\u0100' then
                    i
                elif Char.IsSurrogate(ch) then
                    i - 1
                else
                    loop(i - 1)
            else
                -1
        loop (input.Length - 1)

    let htmlEncode (value: string | null) (sb: StringBuilder) =
        match value with
        | Null ->
            sb.Append(value) |> ignore
        | NonNull value ->
            match indexOfHtmlEncodingCharsBack value with
            | -1 -> sb.Append(value) |> ignore
            | _ -> htmlEncodeInner (value.AsSpan ()) sb
