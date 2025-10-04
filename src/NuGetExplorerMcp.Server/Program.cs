using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NuGetExplorerMcp.Server;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr (required for MCP)
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add NuGet Explorer services
builder.Services.AddNuGetExplorerServices();

// Add MCP server with stdio transport and auto-register tools from assembly
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
