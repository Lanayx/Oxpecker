[<AutoOpen>]
module Components.TodoList

open System
open Oxpecker.Solid

let initialTasks = [|
    {| Id = Guid.NewGuid(); Text = "Drink some coffee" |}
    {| Id = Guid.NewGuid(); Text = "Create a TODO app" |}
    {| Id = Guid.NewGuid(); Text = "Drink some more coffee" |}
|]

[<SolidComponent>]
let TodoList() =
    let getTasks, setTasks = createSignal initialTasks
    let getNewTaskText, setNewTaskText = createSignal ""

    div(){
        h1() { "TODO" }
        div() {
            input(type'="text", placeholder="Enter a task", required = true,
                  class'="flex-1 p-2.5 border-gray-800 border rounded mr-2.5 bg-gray-800 text-gray-200")
            button(class'="py-2.5 px-5 bg-blue-500 text-white border-0 rounded cursor-pointer hover:bg-blue-600") {
                "Add"
            }
        }
        ol(id="todo-list") {
            For(each = getTasks()) {
                fun (task: {| Id: Guid; Text: string |}) _ ->
                    li() {
                        task.Text
                    }
            }
        }
    }
