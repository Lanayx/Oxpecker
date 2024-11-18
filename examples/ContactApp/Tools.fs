module ContactApp.Tools

open System.IO
open System.IO.Compression
open System.Threading.Tasks
open ContactApp.Models
open Microsoft.AspNetCore.Http
open Oxpecker

let getFlashedMessage (ctx: HttpContext) =
    match ctx.Items.TryGetValue("message") with
    | true, msg ->
        ctx.Items.Remove("message") |> ignore
        string msg
    | _ ->
        match ctx.Request.Cookies.TryGetValue("message") with
        | true, NonNull msg ->
            ctx.Response.Cookies.Delete("message")
            msg
        | _ ->
            ""

let flash (msg: string) (ctx: HttpContext) =
    ctx.Items.Add("message", msg)
    ctx.Response.Cookies.Append("message", msg)

let writeHtml view ctx =
    htmlView (view ctx) ctx


type Archiver(contacts: ResizeArray<Contact>) =
    let mutable status = "Waiting"
    let mutable progress = 0.0
    let path = "contacts.zip"

    let createZipArchive stream =
        task {
            use archive = new ZipArchive(stream, ZipArchiveMode.Create, true)
            let file = archive.CreateEntry("contacts.csv")
            use writeStream = file.Open()
            use writer = new StreamWriter(writeStream)
            for contact in contacts do
                do! Task.Delay(500)
                progress <- progress + (1.0 / float contacts.Count)
                writer.WriteLine($"{contact.First},{contact.Last},{contact.Email},{contact.Phone}")
        }

    let writeToFile (stream: MemoryStream) =
        use fs = new FileStream(path, FileMode.Create)
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        stream.CopyTo(fs)

    member this.Status = status
    member this.Progress = progress
    member this.Run() =
        status <- "Running"
        task {
            use stream = new MemoryStream()
            do! createZipArchive stream
            writeToFile stream
            status <- "Complete"
        }
    member this.Reset() =
        status <- "Waiting"
        if File.Exists(path) then
            File.Delete(path)
    member this.ArchiveFile = path
