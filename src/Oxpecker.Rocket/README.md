# Oxpecker.Rocket

This work-in-progress Fable project combines the browser rendering foundation
and shared Datastar attributes for Rocket component authoring. Rocket bindings
are not implemented yet.

The linked sources retain the `Oxpecker.ViewEngine` and `Oxpecker.Datastar`
namespaces and the server-side view syntax. Browser applications reference
`Oxpecker.Rocket`; server applications keep their existing references. Do not
reference the browser and server variants together in the same project.

This project links directly to the original source files. Changes within
those files apply to both targets automatically. When adding a new shared source
file, add it to both project compile lists in the same order. Do not copy tag or
attribute implementations here.

Browser-only `Tools.fs` and `Render.fs` live in this project. The existing .NET
renderer and CI workflow are unchanged. Builders, tags, ARIA, and Datastar
attributes are shared sources rather than copies.

The browser target supports `Render.toString` and `Render.toHtmlDocString`, with
the same DSL and escaping contract. Streams, UTF-8 buffer writers, and the .NET
builder pool remain server-only. It does not yet provide Rocket bindings.

From the repository root, with the local .NET tools restored and Node installed:

```sh
dotnet build Oxpecker.Rocket.slnx
dotnet test tests/Oxpecker.Rocket.Parity.Tests/Oxpecker.Rocket.Parity.Tests.fsproj
dotnet fable tests/Oxpecker.Rocket.Fable.Tests --outDir tests/Oxpecker.Rocket.Fable.Tests/dist --extension .mjs
node tests/Oxpecker.Rocket.Fable.Tests/dist/Program.mjs
```

Both test targets compile `tests/Oxpecker.Rocket.Shared/RenderingCases.fs` and
check the same expected output. These tests run independently of the existing
test projects and CI pipeline. JavaScript execution is required: a .NET build
alone does not verify Fable behavior. Browser lifecycle tests and packaged
consumer verification will accompany the Rocket integration.
