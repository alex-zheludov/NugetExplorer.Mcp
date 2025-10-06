namespace NugetExplorer.IntegrationTests;

public class McpServerFixture : IAsyncLifetime
{
    public string ServerPath { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Build the server project once for all tests
        var projectPath = Path.GetFullPath(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..", "..",
                "src", "NugetExplorer.Mcp", "NugetExplorer.Mcp.csproj"
            )
        );

        Console.WriteLine($"Building server project: {projectPath}");

        var buildProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectPath}\" -c Release",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        if (buildProcess == null)
        {
            throw new InvalidOperationException("Failed to start build process");
        }

        await buildProcess.WaitForExitAsync();

        if (buildProcess.ExitCode != 0)
        {
            var error = await buildProcess.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Build failed with exit code {buildProcess.ExitCode}: {error}");
        }

        Console.WriteLine("Build completed successfully");

        // Get the path to the server executable
        ServerPath = Path.GetFullPath(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..", "..",
                "src", "NugetExplorer.Mcp", "bin", "Release", "net10.0",
                "NugetExplorer.Mcp.dll"
            )
        );

        if (!File.Exists(ServerPath))
        {
            throw new FileNotFoundException($"Server executable not found at: {ServerPath}");
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
