open System
open Microsoft.Extensions.Logging
open ModelContextProtocol.Protocol
open ModelContextProtocol.Client

module Client =

    let execute () =
        task {
            use! mcpClient =
                McpClientFactory.CreateAsync(
                    SseClientTransport(
                        SseClientTransportOptions(
                            Name = "GetCurrentTime",
                            Endpoint = Uri("http://localhost:5000/mcp/sse")
                        )
                    ),
                    McpClientOptions(
                        ClientInfo = Implementation(
                            Name = "UtcTimeTool",
                            Version = "1.0.0"
                        )
                    ),
                    loggerFactory = LoggerFactory.Create(fun builder ->
                        builder.AddConsole().SetMinimumLevel(LogLevel.Information) |> ignore)
                )

            // Retrieve and display available MCP tools.
            let! tools = mcpClient.ListToolsAsync()

            for tool in tools do
                Console.WriteLine($"{tool.Name}: {tool.Description}")

            // Call the UtcTimeTool.
            let! result = tools[0].CallAsync()
            Console.WriteLine(result.Content[0].Text)
        }

[<EntryPoint>]
let main _ =
    Client.execute().Wait()
    0
