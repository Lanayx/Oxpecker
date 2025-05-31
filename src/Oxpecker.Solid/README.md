---
---
# Oxpecker.Solid

Fable (4.23.0+) opinionated bindings for the **Solid.js**, **Solid-router.js** and **Solid-meta.js** libraries based on Oxpecker's view engine. This library DOES NOT depend on `Fable.Solid` package.

**Medium article**: [Oxpecker goes full stack](https://medium.com/@lanayx/oxpecker-goes-full-stack-45beb1f3da34)

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
If you *do* want to use props and have merge and split functionality for integrating third-party library, you should use [Partas.Solid](https://github.com/shayanhabibi/Partas.Solid) library, that heavily relies on them.

### Fragment element

`Fragment` element get transformed in Solid's `<>...</>`tag. It's used to wrap multiple elements without creating a new DOM element.

### For and Index elements

These elements are used to iterate over a list of items (you can read about the difference [here](https://www.solidjs.com/guides/faq#why-shouldnt-i-use-map-in-my-template-and-whats-the-difference-between-for-and-index)). You'll need to use `yield` keyword before the iterating function because currently F# computation expressions don't allow for implicit `yield` for functions.

### Custom HTML tags / Web Components

Custom elements can be useful for integration with Web Component based libraries. You can create one by adding `RegularNode` (or `VoidNode`) marker interface:

```fsharp
namespace Oxpecker.Solid.MyTags

[<CompiledName("my-tag")>]
type MyTag () =
    interface RegularNode
    member this.myAttr
        with set (value: string) = ()
```
Make sure you put your custom element type definition in a separate module (not to the same module with its usage) in a namespace starting from `Oxpecker.Solid` for compiler plugin to transform it correctly.

### Components

There are two ways you can create components in Oxpecker.Solid:

#### 1. Regular functions

This is the most common way to create components and should be used by default.

```fsharp
// without children
[<SolidComponent>]
let Component1 (getText: Accessor<string>) =
    h1(onClick = fun _ -> console.log(getText())) {
        getText()
    }

// with children
[<SolidComponent>]
let Component2 (hello: string) (children: #HtmlElement) =
    h1() {
        hello
        children
    }

// usage
[<SolidComponent>]
let Test () =
    let getText, _ = createSignal "Hello,"

    div() {
        Component1 getText
        Component2 "world" <| span() {
            "!"
        }
    }
```
#### 2. Types

This is useful when you are creating a component with a lot of properties, and you want to make some of the optional, so component user won't have to initialize all of them.

```fsharp
// defniniion
type MyButton() =
    // properties with defaults
    member val btnColor = "" with get, set
    member val onClick: MouseEvent -> unit = ignore with get, set
    member val ariaLabel = "" with get, set

    [<SolidComponent>]
    member this.WithChildren(content: #HtmlElement) = // Can be any name
        button(type'="button", class'= $"text-base ml-2.5 hover:text-blue-500 {this.btnColor}",
               onClick = this.onClick, ariaLabel = this.ariaLabel) {
            content
        }

// usage
[<SolidComponent>]
let Test() =
    div() {
        MyButton(onClick = (fun _ -> alert "Deleted"), btnColor = "text-red-500") // ariaLabel is optional
            .WithChildren(span() { "🗑️" })
    }
```

If you want to build even more complicated components that are compiled into JSX components (rather than JS function calls), heavily depends on props spread, merge and split, you can use [Partas.Solid](https://github.com/shayanhabibi/Partas.Solid) library, which supports all of that.

### Context

This library doesn't provide support for React-like context. I strongly believe it's an antipattern, and encourage you to use global signals or stores instead.
If you *do* want to use Context, have a look at [Partas.Solid](https://github.com/shayanhabibi/Partas.Solid) library, where it is supported.

### Special JSX attributes

Note that `attr:`, `on:`, `bool:`, `ref` attributes are exposed as F# methods in the API: `elem.on(...)`, `elem.attr(...)` etc. Also, `style` and `class'` are attributes when accepting `string`, while `style'` and `classList` are methods when accepting `object` (to be used with [createObj](https://fable.io/docs/javascript/features.html#createobj)).

_Note_: when using `ref(nativeElem)` make sure that `nativeElem` is mutable (e.g. `let mutable nativeElem = Unchecked.defaultof<HTMLDivElement>`).

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

[Router](https://docs.solidjs.com/solid-router) namespace is `Oxpecker.Solid.Router`. It contains router related components and functions. To render a router you still need to decorate router function with `SolidComponent` attribute:
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
_You still need to add a separate reference to @solidjs/router in your package.json._

### Meta

[Meta](https://docs.solidjs.com/solid-meta) namespace is `Oxpecker.Solid.Meta`. It contains elements to be rendered in the application's `<head>` section:
```fsharp
open Oxpecker.Solid.Meta

[<SolidComponent>]
let Root () =
    MetaProivder() {
        Title() { "My App" }
    }
```
_You still need to add a separate reference to @solidjs/meta in your package.json._

### Aria

Similar to the original Oxpecker.ViewEngine additional Aria attributes reside in a separate module, so you need to write `open Oxpecker.Solid.Aria` to access them.

### SVG

Oxpecker supports SVG elements, so you can use them in your components as well. Just make sure to add `open Oxpecker.Solid.Svg` to make it work.

_Note: remember, that you [can't use the same SVG node twice](https://github.com/solidjs/solid/discussions/1134?sort=new#discussioncomment-3226872)_

### Lazy
For components lazy loading you'll need to use `lazy'` function together with another `importComponent` function.
```fsharp
lazy'(fun () -> importComponent("./ComponentA"))
```
will be translated to

```js
lazy(() => import("./ComponentA"))
```
