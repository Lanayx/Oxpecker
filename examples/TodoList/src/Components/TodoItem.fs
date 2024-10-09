[<AutoOpen>]
module Components.TodoItem

open System
open Oxpecker.Solid
open Oxpecker.Solid.Aria


type Task = {| Id: Guid; Text: string |}
type Actions = {| DeleteTask: unit -> unit; MoveTaskUp: unit -> unit; MoveTaskDown: unit -> unit |}

[<SolidComponent>]
let TodoItem (action: Actions) (task: Task) =
    let buttonClass' = "text-base ml-2.5 hover:text-blue-500"
    li(class'= "flex justify-between items-center p-2.5 border-b border-neutral-700 last:border-b-0") {
        span(class'="flex-1") { task.Text }
        button(onClick = (fun _ -> action.DeleteTask()), type'="button", ariaLabel="Delete task",
               class'= $"{buttonClass'} text-red-500") {
            "🗑️"
        }
        button(onClick = (fun _ -> action.MoveTaskUp()), type'="button", ariaLabel="Move task up",
               class'= $"{buttonClass'} text-green-500") {
            "⇧"
        }
        button(onClick = (fun _ -> action.MoveTaskDown()), type'="button", ariaLabel="Move task down",
               class'= $"{buttonClass'} text-green-500") {
            "⇩"
        }
    }
