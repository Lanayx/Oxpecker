module Oxpecker.ViewEngine.Tools

open System
open System.Text


// Browser renders do not use the .NET object pool.
type StringBuilderPoolImpl() =
    member _.Get() = StringBuilder()
    member _.Return(_: StringBuilder) = ()

let StringBuilderPool = StringBuilderPoolImpl()


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


    // Keep the escaping contract identical to the optimized .NET encoder below.
    let htmlEncode (value: string | null) (sb: StringBuilder) =
        match value with
        | null -> ()
        | value ->
            let mutable i = 0
            while i < value.Length do
                let ch = value[i]
                let code = int ch
                match ch with
                | '<' -> sb.Append("&lt;") |> ignore
                | '>' -> sb.Append("&gt;") |> ignore
                | '"' -> sb.Append("&quot;") |> ignore
                | '\'' -> sb.Append("&#39;") |> ignore
                | '&' -> sb.Append("&amp;") |> ignore
                | _ when code >= 0x00A0 && code <= 0x00FF -> sb.Append("&#").Append(code).Append(';') |> ignore
                | _ when code >= 0xD800 && code <= 0xDBFF ->
                    if i + 1 < value.Length && int value[i + 1] >= 0xDC00 && int value[i + 1] <= 0xDFFF then
                        let codePoint = 0x10000 + ((code - 0xD800) * 0x400) + (int value[i + 1] - 0xDC00)
                        sb.Append("&#").Append(codePoint).Append(';') |> ignore
                        i <- i + 1
                    else
                        sb.Append(char 0xFFFD) |> ignore
                | _ when code >= 0xDC00 && code <= 0xDFFF -> sb.Append(char 0xFFFD) |> ignore
                | _ -> sb.Append(ch) |> ignore
                i <- i + 1
