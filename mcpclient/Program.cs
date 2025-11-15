// Import packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using OpenAI.Chat;

// Populate values from your OpenAI deployment
var modelId = "llama3.1:8b";
var endpoint = "http://localhost:11434/v1";
var apiKey = "not-used";

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder();

builder.Services.AddOpenAIChatCompletion(
    modelId: modelId,
    endpoint: new Uri(endpoint),
    apiKey: apiKey);

IClientTransport clientTransport;

// make sure AspNetCoreMcpServer is running
clientTransport = new HttpClientTransport(new()
{
    Endpoint = new Uri("http://localhost:3001")
});

var mcpClient = await McpClient.CreateAsync(clientTransport!);

var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"Connected to server with tools: {tool.Name}");
}

// Convert MCP tools to kernel functions
builder.Plugins.AddFromFunctions("McpTools", tools.Select(t => t.AsKernelFunction()));

// Add enterprise components
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);