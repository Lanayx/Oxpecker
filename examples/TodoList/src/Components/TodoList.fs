[<AutoOpen>]
module Components.TodoList

open Oxpecker.Solid
open Oxpecker.Solid.Aria
open Browser.Types
open Fable.Core.JsInterop


let initialTasks = [|
    {| Id = createUniqueId(); Text = "Drink some coffee" |}
    {| Id = createUniqueId(); Text = "Create a TODO app" |}
    {| Id = createUniqueId(); Text = "Drink some more coffee" |}
|]

[<SolidComponent>]
let TodoList() =
    let tasks, setTasks = createSignal initialTasks
    let newTaskText, setNewTaskText = createSignal ""

    let handleInputChange (event: Event) =
        setNewTaskText(event.target?value)

    let addTask (event: Event) =
        if newTaskText().Trim() <> "" then
            tasks()
            |> Array.insertAt 0 {| Id = createUniqueId(); Text = newTaskText() |}
            |> setTasks
            setNewTaskText("")
        event.preventDefault()

    let deleteTask id =
        tasks()
        |> Array.filter (fun task -> task.Id <> id)
        |> setTasks

    let moveTaskUp index =
        let _tasks = tasks()
        if index > 0 then
            _tasks
            |> Array.mapi (fun i task ->
                if i = index then
                    _tasks[index - 1]
                elif i = index - 1 then
                    _tasks[index]
                else task)
            |> setTasks

    let moveTaskDown index =
        let _tasks = tasks()
        if index < _tasks.Length-1 then
            _tasks
            |> Array.mapi (fun i task ->
                if i = index then
                    _tasks[index + 1]
                elif i = index + 1 then
                    _tasks[index]
                else task)
            |> setTasks

    article(ariaLabel="task list manager",
            class'="bg-neutral-900 p-5 rounded-lg shadow w-full max-w-md"){
        header(){
            h1(class'="text-center text-neutral-300 text-4xl m-6") { "TODO" }
            form(ariaControls="todo-list",
                 class'="flex justify-between mb-5").on("submit", addTask) {
                input(type'="text", placeholder="Enter a task", required = true, ariaLabel = "Task text",
                      value = newTaskText(), onChange = handleInputChange,
                      class'="w-auto flex-1 p-2.5 border-neutral-700 border rounded mr-2.5 bg-neutral-800 text-neutral-200 sm:w-96")
                button(class'="py-2.5 px-5 bg-blue-500 text-white border-0 rounded cursor-pointer hover:bg-blue-600") {
                    "Add"
                }
            }
        }
        ol(id="todo-list", ariaLive="polite", ariaLabel="task list",
           class'="list-none p-0") {
            For(each = tasks()) {
                yield fun task index ->
                    TodoItem {|
                        DeleteTask = fun () -> deleteTask task.Id
                        MoveTaskUp = fun () -> moveTaskUp <| index()
                        MoveTaskDown = fun () -> moveTaskDown <| index()
                    |} task
            }
        }
    }


let store, setStore = createStore {|
    newTaskText = ""
    tasks = initialTasks
|} 

// Alternative implementation using solid store
[<SolidComponent>]
let TodoListStore() =

    let handleInputChange (event: Event) =
        let v: string = event.target?value
        setStore.Path.Map(_.newTaskText).Update(v)

    let addTask (event: Event) =
        if store.newTaskText <> "" then
            let newTasks =
                store.tasks
                |> Array.insertAt 0 {| Id = createUniqueId(); Text = store.newTaskText |}
            setStore.Update {|
                tasks = newTasks
                newTaskText = ""
            |}
        event.preventDefault()

    let deleteTask id =
        let newTasks =
            store.tasks
            |> Array.filter (fun task -> task.Id <> id)
        setStore.Path.Map(_.tasks).Update(newTasks)

    let moveTaskUp index =
        if index > 0 then
            let newTasks =
                store.tasks
                |> Array.mapi (fun i task ->
                    if i = index then
                        store.tasks[index - 1]
                    elif i = index - 1 then
                        store.tasks[index]
                    else task)
            setStore.Path.Map(_.tasks).Update(newTasks)

    let moveTaskDown index =
        if index < store.tasks.Length-1 then
            let newTasks =
                store.tasks
                |> Array.mapi (fun i task ->
                    if i = index then
                        store.tasks[index + 1]
                    elif i = index + 1 then
                        store.tasks[index]
                    else task)
            setStore.Path.Map(_.tasks).Update(newTasks)

    article(ariaLabel="task list manager",
            class'="bg-neutral-900 p-5 rounded-lg shadow w-full max-w-md"){
        header(){
            h1(class'="text-center text-neutral-300 text-4xl m-6") { "TODO" }
            form(ariaControls="todo-list",
                 class'="flex justify-between mb-5").on("submit", addTask) {
                input(type'="text", placeholder="Enter a task", required = true, ariaLabel = "Task text",
                      value = store.newTaskText, onChange = handleInputChange,
                      class'="w-auto flex-1 p-2.5 border-neutral-700 border rounded mr-2.5 bg-neutral-800 text-neutral-200 sm:w-96")
                button(class'="py-2.5 px-5 bg-blue-500 text-white border-0 rounded cursor-pointer hover:bg-blue-600") {
                    "Add"
                }
            }
        }
        ol(id="todo-list", ariaLive="polite", ariaLabel="task list",
           class'="list-none p-0") {
            For(each = store.tasks) {
                yield fun task index ->
                    TodoItem {|
                        DeleteTask = fun () -> deleteTask task.Id
                        MoveTaskUp = fun () -> moveTaskUp <| index()
                        MoveTaskDown = fun () -> moveTaskDown <| index()
                    |} task
            }
        }
    }
