# Oxpecker.ViewEngine

`Oxpecker.ViewEngine` is code-as-markup engine used to render your HTML views based on the F# feature called [Computation Expressions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions).

[Nuget package](https://www.nuget.org/packages/Oxpecker.ViewEngine) `dotnet add package Oxpecker.ViewEngine`

Markup example:

```fsharp
open Oxpecker.ViewEngine

type Person = { Name: string }

let subView = p() { "Have a nice day" }

let mainView (model: Person) =
    html() {
        body(style="width: 800px; margin: 0 auto") {
            h1(style="text-align: center; color: red") {
                $"Hello, {model.Name}!"
            }
            subView
            ul() {
                for i in 1..10 do
                    br()
                    li().attr("onclick", $"alert('Test {i}')") {
                        span(id= $"span{i}", class'="test") { $"Test {i}" }
                    }
            }
        }
    }
```

## Documentation:

- [HtmlElement](#htmlelement)
- [Children](#children)
- [Attributes](#attributes)
- [Event handlers](#event-handlers)
- [Html escaping](#html-escaping)
- [Rendering](#rendering)
- [ARIA](#aria)
- [Fragments](#fragments)

### HtmlElement

`HtmlElement` is a main building block of `Oxpecker.ViewEngine`. It can be constructed from the instance of the `HtmlElementType`:

```fsharp
module NodeType =
    let NormalNode = 0uy
    let VoidNode = 1uy
    let RegularTextNode = 2uy
    let RawTextNode = 3uy
type HtmlElementType = { NodeType: byte; Value: string }
type HtmlElement(elemType: HtmlElementType) =
    ...
```
This represents 4 types of Html elements: normal tags that have opening and closing part, void tags with no closint part and text nodes (which can be escaped and non-escaped strings).

All HTML tags inherit from `HtmlElement` and you can easily create your own tag:

```fsharp
type myTag() =
    inherit HtmlElement("myTag") // this overload creates NormalNode
```

`HtmlElement` holds two collections inside: attributes and children. More on them below.

### Children

Normal nodes can have children that will be added to children collection as you write them between curly braces. Void nodes and Text nodes don't have children. You can programmatically access `Chidren` property of any `HtmlElement`.

```fsharp
let result = div() {
    br()
    span() { "Some text" }
}

let children = result.Children // <br> and <span>
```

### Attributes

Normal and Void nodes can have attributes. Some general attributes are defined inside `HtmlElement` while each tag can have it's specific attributes. This will prevent you from assiging attributes to the element that it doesn't support. You can programmatically access `Attributes` property of any `HtmlElement`.

```fsharp
let result = div(class'="myClass") {
    br(id="1234") // href attribute won't work here
    a(href="/") { "Some link" }
}

let children = result.Attributes // myClass
```
You can also attach _any_ custom attribute to the `HtmlElement` using `.attr` method:

```fsharp
div().attr("my-secret-key", "lk23j4oij234"){
    "Secret div"
}
```
For **data-*** attributes you can use dedicated method:

```fsharp
div().data("secret-key", "lk23j4oij234"){
    "Secret div"
} // renders <div data-secret-key="lk23j4oij234">Secret div</div>
```

### Event handlers

`Oxpecker.ViewEngine` doesn't provide attributes for javascript event handlers like `onclick`. This is done on purpose, since it would encourage people using them, which is rather an antipattern. However, if you really need it, you can always use `.on` method to achieve same goal.

ViewEngine will create html attribute with inline handler for you:

```fsharp
div().on("click", "alert('Hello')"){
    "Clickable div"
}
// <div onclick="alert('Hello')">Clickable div</div>
```


### HTML escaping

`Oxpecker.ViewEngine` will escape text nodes and attribute values for you. However, sometimes it's _desired_ to render unescaped html string, in that case `raw` function is provided

```fsharp
div(){
    "<script></script>" // This will be escaped
    raw "<script></script>" // This will NOT be escaped
}
```

### Rendering

There are several functions to render `HtmlElement` (after opening Oxpecker.ViewEngine namespace):

- **Render.toString** will render to standard .NET UTF16 string
- **Render.toBytes** will render to UTF8-encoded byte array
- **Render.toStreamAsync** will asynchronously render to stream in UTF8 encoding
- **Render.toTextWriterAsync** will asynchronously render to the provided text writer
- **Render.toHtmlDocBytes** is the same as **Render.toBytes**, but will also prepend `"<!DOCTYPE html>"` to the HTML document
- **Render.toHtmlDocString** is the same as **Render.toString**, but will also prepend `"<!DOCTYPE html>"` to the HTML document
- **Render.toHtmlDocStreamAsync** is the same as **Render.toStreamAsync**, but will also prepend `"<!DOCTYPE html>"` to the HTML document
- **Render.toHtmlDocTextWriterAsync** is the same as **Render.toTextWriterAsync**, but will also prepend `"<!DOCTYPE html>"` to the HTML document

### Aria

To enable ARIA attributes support you need to open `Aria` module:

```fsharp
open Oxpecker.ViewEngine.Aria

let x = span(
    role="checkbox",
    id="checkBoxInput",
    ariaChecked="false",
    tabindex=0,
    ariaLabelledBy="chk15-label"
)
```

### Fragments

Sometimes you need to group several elements together without wrapping them in `div` or similar. You can use `__` special tag for that:

```fsharp
let onlyChildren = __() {
    span() { "Some text" }
    span() { "Some other text" }
}

let parent = div() {
    onlyChildren
}

```
