using ModelContextProtocol.Client;
using Shouldly;
using Xunit.Abstractions;

namespace NugetExplorer.IntegrationTests;

public class McpServerTests : IClassFixture<McpServerFixture>, IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly McpServerFixture _fixture;
    private McpClient? _client;

    public McpServerTests(ITestOutputHelper output, McpServerFixture fixture)
    {
        _output = output;
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Create MCP client with stdio transport
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "NuGetExplorerMcp.Server",
            Command = "dotnet",
            Arguments = [_fixture.ServerPath]
        });

        _client = await McpClient.CreateAsync(transport);
    }

    [Fact]
    public async Task Server_Should_List_Tools()
    {
        // Act
        var tools = await _client!.ListToolsAsync();

        // Assert
        tools.ShouldNotBeNull();
        tools.ShouldNotBeEmpty();

        _output.WriteLine($"Found {tools.Count} tools:");
        foreach (var tool in tools)
        {
            _output.WriteLine($"  - {tool.Name}: {tool.Description}");
        }

        tools.ShouldContain(t => t.Name == "list_package_sources");
        tools.ShouldContain(t => t.Name == "analyze_packages");
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
        result.ShouldNotBeNull();
        result.Content.ShouldNotBeEmpty();

        var textContent = result.Content.FirstOrDefault(c => c.Type == "text");
        textContent.ShouldNotBeNull();

        var text = textContent.GetType().GetProperty("Text")?.GetValue(textContent) as string;
        _output.WriteLine($"Package sources result: {text}");

        text.ShouldContain("sources");
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
        result.ShouldNotBeNull();
        result.Content.ShouldNotBeEmpty();

        var textContent = result.Content.FirstOrDefault(c => c.Type == "text");
        textContent.ShouldNotBeNull();

        var text = textContent.GetType().GetProperty("Text")?.GetValue(textContent) as string;
        _output.WriteLine($"Analysis result: {text}");

        text.ShouldContain("packages");
        text.ShouldContain("Newtonsoft.Json");
    }

    public async Task DisposeAsync()
    {
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}
