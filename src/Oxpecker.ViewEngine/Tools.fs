module Oxpecker.ViewEngine.Tools

open System
open System.Buffers
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

    [<Literal>]
    let private UnicodeReplacementChar = '\uFFFD'

    // Function to encode HTML entities
    let private htmlEncodeInner (input: ReadOnlySpan<char>) (sb: StringBuilder) : unit =
        let mutable i = 0
        while i < input.Length do
            let mutable ch = input[i]
            if ch <= '>' then
                if '<' = ch then sb.Append "&lt;"
                elif '>' = ch then sb.Append "&gt;"
                elif '"' = ch then sb.Append "&quot;"
                elif '\'' = ch then sb.Append "&#39;"
                elif '&' = ch then sb.Append "&amp;"
                else sb.Append ch
                |> ignore
                i <- i + 1
            else
                let mutable valueToEncode = -1
                if Char.IsBetween(ch, '\u00A0', '\u00FF') then
                    // The seemingly arbitrary 160 comes from RFC
                    valueToEncode <- int ch
                elif Char.IsSurrogate(ch) then
                    if (i + 1) < input.Length then
                        match Rune.TryCreate(ch, input[i + 1]) with
                        | true, rune ->
                            valueToEncode <- rune.Value
                            i <- i + 1
                        | _ ->
                            // Don't encode BMP characters (like U+FFFD) since they wouldn't have
                            // been encoded if explicitly present in the string anyway.
                            ch <- UnicodeReplacementChar
                    else
                        // Invalid surrogate pair
                        ch <- UnicodeReplacementChar
                if valueToEncode >= 0 then
                    sb.Append("&#").Append(valueToEncode).Append(';') |> ignore
                else
                    sb.Append(ch) |> ignore
                let mutable x = 1 // dirty hack for performance
                i <- i + x
    let private htmlAsciiNonEncodingChars =
        SearchValues.Create(
            "\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\t\n\v\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !#$%()*+,-./0123456789:;=?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\u007f"
        )
    let rec private findEncodingCharLoop i (input: ReadOnlySpan<char>) =
        if i < input.Length then
            let ch = input[i]
            if ch <= '>' then
                if ch = '<' || ch = '>' || ch = '"' || ch = '\'' || ch = '&' then
                    i
                else
                    findEncodingCharLoop (i + 1) input
            elif Char.IsBetween(ch, '\u00A0', '\u00FF') || Char.IsSurrogate(ch) then
                i
            else
                findEncodingCharLoop (i + 1) input
        else
            -1

    // Function to find the index of characters that need HTML encoding
    let indexOfHtmlEncodingChar (input: ReadOnlySpan<char>) =
        match input.IndexOfAnyExcept(htmlAsciiNonEncodingChars) with
        | -1 -> -1
        | index -> findEncodingCharLoop index input

    let htmlEncode (value: string | null) (sb: StringBuilder) =
        match value with
        | Null -> sb.Append(value) |> ignore
        | NonNull value ->
            let value = value.AsSpan()
            match indexOfHtmlEncodingChar value with
            | -1 -> sb.Append(value) |> ignore
            | index ->
                sb.Append(value.Slice(0, index)) |> ignore
                htmlEncodeInner (value.Slice index) sb
