# Building a NuGet MCP Server: Automating Package Management with AI

The open source landscape is changing. More projects are transitioning to commercial models—many rightfully so, as maintainers deserve to profit from their hard work. But keeping track of license changes across dozens of dependencies? That's becoming increasingly difficult. Add to that the need to monitor security vulnerabilities and available updates, and suddenly package management feels like a part-time job.

I recently started tinkering with the [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) and realized it would be a perfect way to streamline my workflow for NuGet package updates. The built-in NuGet explorer interfaces in most IDEs are frustratingly slow, especially when dealing with multiple packages or private feeds. Why not let an AI agent handle the grunt work?

## The Problem

Managing NuGet packages typically involves:
- Opening the slow NuGet package manager UI
- Manually checking each package for updates
- Hunting down CVE information for security vulnerabilities
- Investigating license changes when you remember to do it (which is rare)
- Repeating this process across multiple projects

It's tedious, time-consuming, and easy to procrastinate on—especially the security and licensing checks.

## The Solution: A NuGet MCP Server

I built a simple MCP server that exposes NuGet package analysis capabilities to AI assistants. Now I can just ask Claude: *"Check this solution for package updates and vulnerabilities"* and get a comprehensive analysis in seconds.

The server provides two main tools:

### 1. `analyze_packages`
Performs comprehensive package analysis including:
- **Update detection** - Checks for newer stable and prerelease versions
- **Vulnerability scanning** - Queries the [GitHub Advisory Database](https://github.com/advisories) for known CVEs
- **License change detection** - Alerts when licenses change between your current version and available updates
- **Private feed support** - Works with your configured `nuget.config` (Azure Artifacts, GitHub Packages, etc.)

### 2. `list_package_sources`
Lists all configured NuGet sources from your machine, showing which feeds are enabled and authenticated.

## Current State

The core functionality is working well. I can analyze packages, check for updates, and get basic vulnerability information. However, the vulnerability scanning still needs work. License change detection also needs refinement. These are features I plan to implement properly in future iterations.

## How Easy is MCP Implementation?

One of the pleasant surprises was discovering how straightforward it is to build MCP servers using the [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) NuGet package. Here's essentially all the code needed to wire up the MCP server:

```csharp
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr (MCP requirement)
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register your domain services
builder.Services.AddNuGetExplorerServices();

// Add MCP server with stdio transport and auto-discover tools
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
```

Defining tools is equally simple—just attribute your methods:

```csharp
[McpServerToolType]
public static class NuGetTools
{
    [McpServerTool(Name = "analyze_packages")]
    [Description("Comprehensive analysis of NuGet packages...")]
    public static async Task<string> AnalyzePackages(
        IPackageAnalyzer packageAnalyzer,
        [Description("Array of packages to analyze")] PackageInput[] packages,
        [Description("Target framework (e.g., net8.0)")] string? targetFramework = null,
        [Description("Include prerelease versions")] bool includePrerelease = false,
        // ... other parameters
        CancellationToken cancellationToken = default)
    {
        var result = await packageAnalyzer.AnalyzePackagesAsync(/* ... */);
        return JsonSerializer.Serialize(result);
    }
}
```

The framework handles:
- JSON-RPC protocol communication
- Parameter validation and deserialization
- Tool discovery and registration
- Dependency injection
- Error handling

You just write normal C# methods and let the library handle the MCP plumbing.

## Installation and Usage

I installed it in Claude Code using this command (on Windows):

```bash
claude mcp add-json nuget-explorer '{"type":"stdio","command":"cmd","args":["/c","dotnet run --project [PATH TO REPOS]/Nuget.Mcp/src/NuGetExplorerMcp.Server/NuGetExplorerMcp.Server.csproj --no-build -c Release"],"env":{}}' --scope user
```

After building the project (`dotnet build -c Release`), I can simply ask Claude to check for package updates in any solution, and it uses the MCP tools to analyze the packages, check for vulnerabilities, and report findings—all without leaving my conversation with the AI.

## Try It Yourself

Want to give it a shot? Clone the repository:

```
https://github.com/alex-zheludov/Nuget.Mcp
```

Build it, configure it with your AI assistant (Claude Desktop, Claude Code, or GitHub Copilot—see the [README](README.md) for specific instructions), and start delegating your package management tasks to AI.

## What's Next

I'm planning to publish this to the new [NuGet MCP feed](https://learn.microsoft.com/en-us/nuget/concepts/nuget-mcp) once it's more mature. That'll make installation even simpler—just a `dotnet tool install` away.

## Final Thoughts

I realize this project likely won't gain much traction and official NuGet MCP server will eventually provide all this functionality and more. But that's not really the point.

This has been an excellent learning opportunity. MCP is a genuinely useful protocol for extending AI capabilities with domain-specific tools. The developer experience is solid, the abstractions make sense, and the integration with .NET feels natural.

I'm looking forward to building more MCP servers for other pain points in my development workflow. Sometimes the best way to learn something new is to solve a problem you actually have — even if it's a small one.
