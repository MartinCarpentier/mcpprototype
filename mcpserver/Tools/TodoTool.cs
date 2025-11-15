using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TodoMcpServer.Tools;

[McpServerToolType]
public sealed class TodoTools
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    // ---------------------------
    // GET /todoitems
    // ---------------------------
    [McpServerTool, Description("Get all todo items.")]
    public static async Task<string> ListTodos(IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("TodoApi");

        using var json = await client.ReadJsonDocumentAsync("/todoitems");
        return JsonSerializer.Serialize(json.RootElement, _jsonOptions);
    }

    // ---------------------------
    // GET /todoitems/{id}
    // ---------------------------
    [McpServerTool, Description("Get a single todo item by ID.")]
    public static async Task<string> GetTodo(
        IHttpClientFactory httpClientFactory,
        [Description("ID of the todo item.")] int id)
    {
        var client = httpClientFactory.CreateClient("TodoApi");
        using var json = await client.ReadJsonDocumentAsync($"/todoitems/{id}");
        return JsonSerializer.Serialize(json.RootElement, _jsonOptions);
    }

    // ---------------------------
    // POST /todoitems
    // ---------------------------
    [McpServerTool, Description("Create a new todo item.")]
    public static async Task<string> CreateTodo(
        IHttpClientFactory httpClientFactory,
        [Description("Name of the todo item.")] string name,
        [Description("Whether the todo item is complete. This value must be set as a boolean value")] string isComplete)
    {
        var client = httpClientFactory.CreateClient("TodoApi");
        var body = new { name, isComplete = bool.Parse(isComplete) };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/todoitems", content);
        response.EnsureSuccessStatusCode();

        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return JsonSerializer.Serialize(json.RootElement, _jsonOptions);
    }

    // ---------------------------
    // PUT /todoitems/{id}
    // ---------------------------
    [McpServerTool, Description("Update an existing todo item.")]
    public static async Task<string> UpdateTodo(
        IHttpClientFactory httpClientFactory,
        [Description("ID of the todo item.")] int id,
        [Description("New name.")] string name,
        [Description("Whether the todo item is complete. This value must be set as a boolean value")] string isComplete)
    {
        var client = httpClientFactory.CreateClient("TodoApi");
        var body = new { name, isComplete = bool.Parse(isComplete)};
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"/todoitems/{id}", content);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new McpException($"Todo item {id} not found.");

        response.EnsureSuccessStatusCode();
        return "Todo updated.";
    }

    // ---------------------------
    // DELETE /todoitems/{id}
    // ---------------------------
    [McpServerTool, Description("Delete a todo item.")]
    public static async Task<string> DeleteTodo(
        IHttpClientFactory httpClientFactory,
        [Description("ID of the todo item.")] int id)
    {
        var client = httpClientFactory.CreateClient("TodoApi");
        var response = await client.DeleteAsync($"/todoitems/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new McpException($"Todo item {id} not found.");

        response.EnsureSuccessStatusCode();
        return "Todo deleted.";
    }
}
