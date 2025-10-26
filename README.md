---
---

# Oxpecker

![Oxpecker](https://github.com/Lanayx/Oxpecker/raw/main/images/oxpecker.png)

**Welcome to Oxpecker!** An F# library designed to supercharge your web development with ASP.NET Core, HTMX, and Solid.js. Oxpecker began as a functional wrapper for ASP.NET Core Endpoint routing (think F#-friendly "Minimal APIs") and has evolved into a comprehensive fullstack solution. Our monorepo includes all related projects, with documentation conveniently located in `README.md` files for each project.

🚀 **Performance Alert!** Oxpecker is recognized as one of the fastest .NET web frameworks in several categories [in the TechEmpower benchmark](https://www.techempower.com/benchmarks/#section=data-r23&p=zik0zj-zik0zj-zijocf-zik0zj-zik0zj-18y67).

The server-side core of Oxpecker is a refined version of the acclaimed [Giraffe](https://github.com/giraffe-fsharp/Giraffe), largely maintaining Giraffe's successful API (hence the name!). Key enhancements include optimized core types, better performance, simplified handler logic, and the removal of outdated functionalities.

## Why Choose Oxpecker?

*   **Blazing Fast:** Benefit from top-tier performance, as validated by TechEmpower benchmarks.
*   **Functional & F#-Friendly:** Enjoy an idiomatic F# experience on top of ASP.NET Core.
*   **Fullstack Power:** Seamlessly integrate backend and frontend with HTMX and Solid.js.
*   **Modern ASP.NET Core:** Leverages native Endpoint routing for robust and flexible web applications.
*   **Rich Feature Set:** From view engines to HTMX integration, Oxpecker has you covered.

## Dive Deeper - Features:

*   Native [ASP.NET Core Endpoint routing](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing) integration
*   Fast ViewEngine with a convenient DSL for HTML
*   OpenAPI integration
*   HTMX integration
*   Strongly typed route parameters
*   Endpoint handlers and endpoint middlewares, offering flexible composition
*   Effortless JSON binding and serialization
*   Simplified Form binding
*   Built-in Model validation
*   Easy URL parameters binding
*   Response caching capabilities
*   Streaming support
*   Authorization mechanisms
*   eTag support
*   Comprehensive IResult support
*   A wealth of useful **HttpContext** extensions

## Get Started Quickly:

Ready to build something amazing?
*   [Empty template](https://github.com/Lanayx/Oxpecker/blob/main/examples/Empty) - Your basic starting point for server apps.
*   [Empty Solid template](https://github.com/Lanayx/Oxpecker/blob/main/examples/EmptySolid) - For frontend Solid.js based applications.

## Explore Our Examples:

See Oxpecker in action:
*   [Fullstack CRUD](https://github.com/Lanayx/Oxpecker/blob/main/examples/CRUD) - Example with functional DI on the backend.
*   [HTMX Contact App](https://github.com/Lanayx/Oxpecker/tree/main/examples/ContactApp) - A sample HTMX application.
*   [ASP.NET WeatherApp (Oxpecker style)](https://github.com/Lanayx/Oxpecker/tree/main/examples/WeatherApp)
*   [Frontend SPA (TodoList)](https://github.com/Lanayx/Oxpecker/tree/main/examples/TodoList) - SPA application without a backend.
*   [MCP Server & Client](https://github.com/Lanayx/Oxpecker/tree/main/examples/MCP) - Model Context Protocol example.
*   [Feature Showcase](https://github.com/Lanayx/Oxpecker/blob/main/examples/Basic) - A dump of different server features all in one place.

## Learn More - Articles & Insights:

Medium posts from the creator:
*   [Oxpecker Introduction](https://medium.com/@lanayx/the-oxpecker-ef9df3dfb918)
*   [7 reasons to try Oxpecker.ViewEngine](https://medium.com/@lanayx/7-reasons-to-try-oxpecker-viewengine-af642b4d191c)
*   [Oxpecker vs Blazor](https://medium.com/@lanayx/blazor-vs-oxpecker-067cbcda9f99)
*   [Oxpecker.Solid introduction](https://medium.com/@lanayx/oxpecker-goes-full-stack-45beb1f3da34)

## Comprehensive Documentation:

*   [Oxpecker](https://lanayx.github.io/Oxpecker/src/Oxpecker/)
*   [Oxpecker.ViewEngine](https://lanayx.github.io/Oxpecker/src/Oxpecker.ViewEngine/)
*   [Oxpecker.Htmx](https://lanayx.github.io/Oxpecker/src/Oxpecker.Htmx/)
*   [Oxpecker.OpenApi](https://lanayx.github.io/Oxpecker/src/Oxpecker.OpenApi/)
*   [Oxpecker.Solid](https://lanayx.github.io/Oxpecker/src/Oxpecker.Solid/)
*   [Migrating from Giraffe](https://lanayx.github.io/Oxpecker/MigrateFromGiraffe)

## Contributing & Development Lifecycle:

*   **`develop` branch:** This is our active development branch. Projects are linked using project references. Please send your Pull Requests here!
*   **`main` branch:** This is our production branch. Projects and examples use NuGet (or npm) packages. Releases are published from this branch.

We welcome contributions! Check out the `develop` branch to get started.

## Oxpecker community space

If you have developed a library that extends Oxpecker functionality, but it can't go into the main repo, you can submit it as a separate repository under [Oxpecker-Community GitHub organization](https://github.com/Oxpecker-Community).

## Official Packages:

Find Oxpecker on NuGet:
*   [Oxpecker](https://www.nuget.org/packages/Oxpecker)
*   [Oxpecker.ViewEngine](https://www.nuget.org/packages/Oxpecker.ViewEngine)
*   [Oxpecker.Htmx](https://www.nuget.org/packages/Oxpecker.Htmx)
*   [Oxpecker.OpenApi](https://www.nuget.org/packages/Oxpecker.OpenApi)
*   [Oxpecker.Solid](https://www.nuget.org/packages/Oxpecker.Solid)
*   [Oxpecker.Solid.FablePlugin](https://www.nuget.org/packages/Oxpecker.Solid.FablePlugin)

## ❤️ Support Oxpecker's Growth!

Oxpecker is a passion project, and your support can make a huge difference! If you find Oxpecker valuable, or if you'd like to see it continue to evolve and improve, please consider becoming a sponsor.

Your contributions help us:
*   Dedicate more time to development and new features.
*   Improve documentation and examples.
*   Provide faster community support.
*   Keep the project healthy and actively maintained.

👉 **[Become a Sponsor on GitHub](https://github.com/sponsors/Lanayx)**

Every bit of support is greatly appreciated and helps us build a better future for F# web development with Oxpecker!
