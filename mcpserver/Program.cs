using MCPServerTest.Tools;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickstartWeatherServer.Tools;
using Serilog;
using System.Net.Http.Headers;
using TodoMcpServer.Tools;

Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Verbose() // Capture all log levels
           .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Verbose)
           .CreateLogger();

try
{
    Log.Information("Starting server...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSerilog();

    // Configure HttpClientFactory for weather.gov API
    builder.Services.AddHttpClient<TodoTools>("TodoApi", client =>
    {
        client.BaseAddress = new Uri("http://localhost:3002");
    });

    builder.Services.AddMcpServer()
        .WithHttpTransport()
        .WithTools<CalculatorTools>()
        .WithTools<RandomNumberTools>()
        .WithTools<TodoTools>()
        .WithTools<EchoTool>()
        .WithTools<WeatherTools>();

    var app = builder.Build();

    app.MapMcp();

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
