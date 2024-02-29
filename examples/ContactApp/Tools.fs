module ContactApp.Tools

open Microsoft.AspNetCore.Http
open Oxpecker

let isNotNull x = not (isNull x)

let getFlashedMessage (ctx: HttpContext) =
    match ctx.Items.TryGetValue("message") with
    | true, msg ->
        ctx.Items.Remove("message") |> ignore
        string msg
    | _ ->
        match ctx.Request.Cookies.TryGetValue("message") with
        | true, msg ->
            ctx.Response.Cookies.Delete("message")
            msg
        | _ -> null

let flash (msg: string) (ctx: HttpContext) =
    ctx.Items.Add("message", msg)
    ctx.Response.Cookies.Append("message", msg)

let writeHtml view ctx =
    htmlView (view ctx) ctx
