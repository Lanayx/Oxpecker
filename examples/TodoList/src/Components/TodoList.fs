[<AutoOpen>]
module Components.TodoList

open System
open Oxpecker.Solid
open Oxpecker.Solid.Aria

type Task = {| Id: Guid; Text: string |}

let initialTasks = [|
    {| Id = Guid.NewGuid(); Text = "Drink some coffee" |}
    {| Id = Guid.NewGuid(); Text = "Create a TODO app" |}
    {| Id = Guid.NewGuid(); Text = "Drink some more coffee" |}
|]

[<SolidComponent>]
let TodoList() =
    let getTasks, setTasks = createSignal initialTasks
    let getNewTaskText, setNewTaskText = createSignal ""

    article(ariaLabel="task list manager",
            class'="bg-neutral-900 p-5 rounded-lg shadow w-full max-w-md"){
        header(){
            h1(class'="text-center text-neutral-300 text-4xl m-6") { "TODO" }
            form(ariaControls="todo-list",
                 class'="flex justify-between mb-5") {
                input(type'="text", placeholder="Enter a task", required = true, ariaLabel = "Task text",
                      value = getNewTaskText(),
                      class'="w-auto flex-1 p-2.5 border-neutral-700 border rounded mr-2.5 bg-neutral-800 text-neutral-200 sm:w-96")
                button(class'="py-2.5 px-5 bg-blue-500 text-white border-0 rounded cursor-pointer hover:bg-blue-600") {
                    "Add"
                }
            }
        }
        ol(id="todo-list", ariaLive="polite", ariaLabel="task list",
           class'="list-none p-0") {
            For(each = getTasks()) {
                fun (task: Task) _ ->
                    li(class'= "flex justify-between items-center p-2.5 border-b border-neutral-700 last:border-b-0") {
                        span(class'="flex-1") { task.Text }
                    }
            }
        }
    }
