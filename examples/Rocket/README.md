# Rocket + Datastar example

A standalone example of the Oxpecker DSL on both sides:

- `Client/Program.fs` authors a Rocket counter in F#, compiled to JavaScript with Fable.
  Two instances demonstrate independent component-local signals, a numeric `start`
  prop, click handlers, and a conditional template.
- `Server/Program.fs` renders the page and a `/hello` HTML fragment using Oxpecker
  and the shared Datastar attributes. `@get('/hello')` patches the element with the
  matching ID, following the [Datastar getting-started guide](https://data-star.dev/guide/getting_started).
  No SSE SDK is needed for this single-response example.

The server and client are separate projects: do not reference `Oxpecker.Rocket`
alongside the server ViewEngine in one project. Existing solutions and CI are unchanged.

## Run

Use the .NET SDK selected by the repository's `global.json`. From the repository root:

```sh
dotnet run --project examples/Rocket/Server -- --urls http://localhost:5067
```

Open <http://localhost:5067>. No npm installation or bundler is required.
An internet connection is required for the pinned Rocket v1.0.4 CDN import.
Its bundle includes Datastar; do not load Datastar separately.
The server build restores the local .NET tools and compiles the Fable client
automatically, including when publishing. Generated JavaScript is ignored by Git.
`--no-build` skips client compilation as well as the server build.

For client development, run Fable in watch mode from the repository root in
another terminal and refresh the browser after changes:

```sh
dotnet fable watch examples/Rocket/Client --outDir examples/Rocket/Server/wwwroot/dist --extension .js
```

## Manual smoke test

1. The counters initially display 0 and 3.
2. Increase/decrease either counter; the other is unchanged and neither drops below 0.
3. At 5, “Five or more!” appears; below 5, it disappears.
4. Click “Say hello from the server”. The greeting and server timestamp appear
   without a page reload or either counter resetting. Click again to update the time.
5. In developer tools, confirm `/hello` returns `text/html` and the console has no errors.

This is a runnable integration example, not an automated browser test.
