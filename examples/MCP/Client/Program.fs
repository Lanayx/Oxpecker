open System
open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.SemanticKernel
open Microsoft.SemanticKernel.Connectors.OpenAI
open ModelContextProtocol.Protocol
open ModelContextProtocol.Client

module Root =

    type LoggingHandler(innerHandler: HttpMessageHandler) =
        inherit DelegatingHandler(innerHandler)
        member this.BaseSendAsync(request, token) =
            base.SendAsync(request, token)
        override this.SendAsync(request, cancellationToken) =
            task {
                if request.Content |> isNull |> not then
                    let! req = request.Content.ReadAsStringAsync(cancellationToken)
                    Console.WriteLine($"Request: {req}")
                let! response = this.BaseSendAsync(request, cancellationToken)
                if response.Content |> isNull |> not then
                    let! res = response.Content.ReadAsStringAsync(cancellationToken)
                    Console.WriteLine($"Response: {res}")
                return response
            }

    let execute () =
        task {
            // Local Ollama model
            let model = "llama3.2:3b"

            use socketHandler =
                new SocketsHttpHandler(
                    PooledConnectionLifetime = TimeSpan.FromMinutes 15.0,
                    ConnectTimeout = TimeSpan.FromSeconds 15.0
                )
            use loggingHandler = new LoggingHandler(socketHandler)
            // HttpClient with request/response logging
            use httpClient = new HttpClient(loggingHandler,
                BaseAddress = Uri("http://localhost:11434/v1") // Ollama API base address
            )

            let kernelBuilder = Kernel.CreateBuilder()
            kernelBuilder.Services.AddLogging() |> ignore
            let kernel =
                Kernel.CreateBuilder()
                    .AddOpenAIChatCompletion(model, "ollama", httpClient = httpClient, serviceId = model)
                    .Build()

            use! mcpClient =
                McpClientFactory.CreateAsync(
                    SseClientTransport(
                        SseClientTransportOptions(
                            Name = "GetCurrentDate",
                            Endpoint = Uri("http://localhost:5000/mcp/sse") // MCP server endpoint
                        )
                    ),
                    McpClientOptions(
                        ClientInfo = Implementation(
                            Name = "CurrentDateTool",
                            Version = "1.0.0"
                        )
                    ),
                    loggerFactory = LoggerFactory.Create(fun builder ->
                        builder.AddConsole().SetMinimumLevel(LogLevel.Trace) |> ignore)
                )

            // Retrieve and display available MCP tools.
            let! tools = mcpClient.ListToolsAsync()

            for tool in tools do
                Console.WriteLine($"{tool.Name}: {tool.Description}")

            // Add MCP tools as plugins to the Semantic Kernel.
            kernel.Plugins.AddFromFunctions("CurrentDateTool", tools |> Seq.map _.AsKernelFunction()) |> ignore

            let userPrompt =
                "I would like to know what date is it and 5 significant things that happened in the past on this day."

            let promptExecutionSettings =
                OpenAIPromptExecutionSettings(
                    Temperature = 0.0,
                    FunctionChoiceBehavior =
                        FunctionChoiceBehavior.Auto(
                            options = FunctionChoiceBehaviorOptions(RetainArgumentTypes = true)
                        )
                )

            let! _ =  kernel.InvokePromptAsync(userPrompt, KernelArguments(promptExecutionSettings))

            Console.WriteLine("The end")
        }

[<EntryPoint>]
let main _ =
    Root.execute().Wait()
    0
