---
---
# Oxpecker.Solid

Fable (4.23.0+) bindings for the **Solid.js** and **Solid-router.js** libraries based on Oxpecker's view engine. This library DOES NOT depend on `Fable.Solid` package.

Introduction post: https://medium.com/@lanayx/oxpecker-goes-full-stack-45beb1f3da34

[Nuget package](https://www.nuget.org/packages/Oxpecker.Solid) `dotnet add package Oxpecker.Solid`

Markup example:

```fsharp
open Oxpecker.Solid // this library namespace
open Browser // Fable browser bindings (needed for console.log)

[<SolidComponent>] // this attribute is required to compile components to JSX that Solid understands
let itemList items = // regular function arguments, no props!
    let x, setX = createSignal 0 // just sample of reactive state usage
    ul() { // ul tag
        For(each = items) { // Solid's built-in For component
            yield fun item index -> // function called for each item
                li() { // li tag
                    span() { index() } // index is reactive getter from Solid
                    span() { item.Name } // string field inside span tag
                    button(onClick=fun _ -> console.log("Removing item...")) { // onClick event handler
                        "Remove" // button text
                    }
                }
        }
    }
```

## Documentation:

You mostly need to refer to the official site for the Solid.js documentation: https://docs.solidjs.com.

You can also check [TODO list sample](https://github.com/Lanayx/Oxpecker/tree/develop/examples/TodoList) in the repository to get a better understanding of how the code will look like in your application.

There are some details that are specific to Oxpecker's view engine implementation:

### SolidComponent attribute

This attribute tells `Fable` compiler plugin (`Oxpecker.Solid.FablePlugin`) that this function should be transformed into Solid compatible JSX during build time. It has no effect at runtime. This plugin is referenced by `Oxpecker.Solid` package and will be automatically added to your project when you install it, no manual reference is needed.

### Props

With this library you can forget about props and special helpers created for managing them. You can use regular function arguments instead. You can be as creative as you want with your function arguments, they can be records, functions, ref cells (especially useful for passing refs from child to parent) or any other regular F# types.

### Fragment element

`Fragment` element get transformed in Solid's `<>...</>`tag. It's used to wrap multiple elements without creating a new DOM element.

### For and Index elements

These elements are used to iterate over a list of items (you can read about the difference [here](https://www.solidjs.com/guides/faq#why-shouldnt-i-use-map-in-my-template-and-whats-the-difference-between-for-and-index)). You'll need to use `yield` keyword before the iterating function because currently F# computation expressions don't allow for implicit `yield` for functions.

### Context

This library doesn't provide support for React-like context. I strongly believe it's an antipattern, and encourage you to use global signals or stores instead.

### Special JSX attributes

Note that `attr:`, `on:`, `bool:`, `ref` attributes are exposed as F# methods in the API: `elem.on(...)`, `elem.attr(...)` etc. Also, `style` and `class'` are attributes when accepting `string`, while `style'` and `classList` are methods when accepting `object`.

### Store

Similar to the original implementation in `Fable.Solid` , store has a special helper for updating its fields:
```fsharp
setStore // store setter
    .Path // special property to access store fields
    .Map(_.data) // choose which field to update
    .Update(newData) // update field with a new value
```

### Resource

Again, just as in the original implementation in `Fable.Solid`, resource is a special object, so instead of JS `resource()` call, you'll need to use `resource.current` in F#.



### Router

Router namespace is `Oxpecker.Solid.Router`. It contains router related components and functions. To render a router you still need to decorate router function with `SolidComponent` attribute:
```fsharp
open Oxpecker.Solid.Router

[<SolidComponent>]
let MyRouter () =
    Router() {
        Route(path="/", component'=Home)
        Route(path="/about", component'=About)
    }

render (MyRouter, document.getElementById "root")
```

### Aria

Similar to the original Oxpecker.ViewEngine additional Aria attributes reside in a separate module, so you need to write `open Oxpecker.Solid.Aria` to access them.


### Lazy
For components lazy loading you'll need to use `lazy'` function together with another `importComponent` function.
```fsharp
lazy'(fun () -> importComponent("./ComponentA"))
```
will be translated to

```js
lazy(() => import("./ComponentA"))
```
