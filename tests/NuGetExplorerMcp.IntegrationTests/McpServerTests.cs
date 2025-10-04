using ModelContextProtocol.Client;
using Xunit.Abstractions;

namespace NuGetExplorerMcp.IntegrationTests;

public class McpServerTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private McpClient? _client;

    public McpServerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        // Get the path to the server executable
        var serverPath = Path.GetFullPath(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..", "..",
                "src", "NuGetExplorerMcp.Server", "bin", "Release", "net8.0",
                "NuGetExplorerMcp.Server.dll"
            )
        );

        if (!File.Exists(serverPath))
        {
            throw new FileNotFoundException($"Server executable not found at: {serverPath}");
        }

        // Create MCP client with stdio transport
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "NuGetExplorerMcp.Server",
            Command = "dotnet",
            Arguments = [serverPath]
        });

        _client = await McpClient.CreateAsync(transport);
    }

    [Fact]
    public async Task Server_Should_List_Tools()
    {
        // Act
        var tools = await _client!.ListToolsAsync();

        // Assert
        Assert.NotNull(tools);
        Assert.NotEmpty(tools);

        _output.WriteLine($"Found {tools.Count} tools:");
        foreach (var tool in tools)
        {
            _output.WriteLine($"  - {tool.Name}: {tool.Description}");
        }

        Assert.Contains(tools, t => t.Name == "list_package_sources");
        Assert.Contains(tools, t => t.Name == "analyze_packages");
    }

    [Fact]
    public async Task Server_Should_List_Package_Sources()
    {
        // Act
        var result = await _client!.CallToolAsync(
            "list_package_sources",
            new Dictionary<string, object?>(),
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Content);

        var textContent = result.Content.FirstOrDefault(c => c.Type == "text");
        Assert.NotNull(textContent);

        var text = textContent.GetType().GetProperty("Text")?.GetValue(textContent) as string;
        _output.WriteLine($"Package sources result: {text}");

        Assert.Contains("sources", text);
    }

    [Fact]
    public async Task Server_Should_Analyze_Packages()
    {
        // Act
        var result = await _client!.CallToolAsync(
            "analyze_packages",
            new Dictionary<string, object?>
            {
                ["packages"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "Newtonsoft.Json",
                        ["version"] = "12.0.0"
                    }
                },
                ["targetFramework"] = "net8.0",
                ["includePrerelease"] = false,
                ["checkUpdates"] = true,
                ["checkVulnerabilities"] = true,
                ["checkLicenses"] = true
            },
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Content);

        var textContent = result.Content.FirstOrDefault(c => c.Type == "text");
        Assert.NotNull(textContent);

        var text = textContent.GetType().GetProperty("Text")?.GetValue(textContent) as string;
        _output.WriteLine($"Analysis result: {text}");

        Assert.Contains("packages", text);
        Assert.Contains("Newtonsoft.Json", text);
    }

    public async Task DisposeAsync()
    {
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}
