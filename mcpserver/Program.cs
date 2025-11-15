using MCPServerTest.Tools;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickstartWeatherServer.Tools;
using Serilog;
using System.Net.Http.Headers;

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
    builder.Services.AddHttpClient("WeatherApi", client =>
    {
        client.BaseAddress = new Uri("https://api.weather.gov");
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
    });

    builder.Services.AddMcpServer()
        .WithHttpTransport()
        .WithTools<RandomNumberTools>()
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
