# NuGet Explorer MCP

A Model Context Protocol (MCP) server for NuGet package analysis. Enables AI assistants to check packages for updates, security vulnerabilities, and license changes using your configured NuGet feeds.

## Features

- **Update Detection** - Check if newer package versions exist (stable & prerelease)
- **Vulnerability Scanning** - Scan for known CVEs via GitHub Advisory Database
- **License Change Detection** - Alert when licenses change between versions
- **Private Feed Support** - Works with nuget.config (Azure Artifacts, GitHub Packages, etc.)
- **Intelligent Caching** - Optimized for performance with configurable TTLs

## MCP Tools

### 1. `analyze_packages`

Comprehensive package analysis in a single call.

**Parameters:**
```json
{
  "packages": [
    { "id": "Newtonsoft.Json", "version": "13.0.1" },
    { "id": "Serilog", "version": "3.0.0" }
  ],
  "targetFramework": "net8.0",
  "includePrerelease": false,
  "checkUpdates": true,
  "checkVulnerabilities": true,
  "checkLicenses": true,
  "severityFilter": "all"
}
```

**Returns:**
```json
{
  "summary": {
    "totalPackages": 2,
    "packagesWithUpdates": 1,
    "vulnerablePackages": 1,
    "packagesWithLicenseChanges": 0,
    "upToDate": 1,
    "severityCounts": {
      "critical": 0,
      "high": 1,
      "medium": 0,
      "low": 0
    }
  },
  "packages": [...]
}
```

### 2. `list_package_sources`

List configured NuGet package sources.

**Parameters:** None

**Returns:**
```json
{
  "sources": [
    {
      "name": "nuget.org",
      "url": "https://api.nuget.org/v3/index.json",
      "isEnabled": true,
      "isOfficial": true,
      "requiresAuth": false,
      "isAuthenticated": false
    }
  ]
}
```

## Configuration

The server reads NuGet package sources from the standard `nuget.config` locations:

- **Windows**: `%AppData%\NuGet\NuGet.Config`
- **Mac/Linux**: `~/.nuget/NuGet/NuGet.Config`
- **Machine-wide**: `%ProgramData%\NuGet\Config\` (Windows)

### Private Feeds

Works seamlessly with private feeds (Azure Artifacts, GitHub Packages, etc.):

```xml
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="CompanyFeed" value="https://pkgs.dev.azure.com/company/_packaging/feed/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

Authentication is handled via NuGet's credential provider mechanism.

## Installation

### 1. Build the Server

```bash
dotnet build -c Release
```

### 2. Configure MCP Client

Replace `[PATH_TO_REPO]` with the absolute path to where you cloned this repository.

#### Claude Desktop / Claude Code

**Windows:**
```bash
claude mcp add-json nuget-explorer '{"type":"stdio","command":"cmd","args":["/c","dotnet run --project [PATH_TO_REPO]/src/NugetExplorer.Mcp/NugetExplorer.Mcp.csproj --no-build -c Release"],"env":{}}' --scope user
```

**Mac/Linux:**
```bash
claude mcp add nuget-explorer -- dotnet run --project [PATH_TO_REPO]/src/NugetExplorer.Mcp/NugetExplorer.Mcp.csproj --no-build -c Release
```

See [Claude MCP Setup Guide](https://docs.claude.com/en/docs/mcp/servers)

#### GitHub Copilot

Add to `.vscode/mcp.json` (repository-specific) or VS Code `settings.json` (global):

```json
{
  "github.copilot.chat.mcp.servers": {
    "nuget-explorer": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "[PATH_TO_REPO]/src/NugetExplorer.Mcp/NugetExplorer.Mcp.csproj",
        "--no-build",
        "-c",
        "Release"
      ]
    }
  }
}
```

See [GitHub Copilot MCP Documentation](https://docs.github.com/en/copilot/how-tos/provide-context/use-mcp/extend-copilot-chat-with-mcp)


## Usage

Once configured, ask your AI assistant:

- *"Check packages in this solution for updates"*
- *"Scan my packages for security vulnerabilities"*
- *"Analyze these packages for updates, vulnerabilities, and license changes"*
- *"What NuGet sources are configured on my machine?"*

The assistant will use the MCP tools to return structured analysis including updates, vulnerabilities, license changes, and compatibility information.


## Caching Strategy

Optimized for performance with in-memory caching:

| Data Type | TTL | Cache Key Pattern |
|-----------|-----|-------------------|
| Package Metadata | 1 hour | `pkg:{packageId}:{version}` |
| All Versions | 1 hour | `versions:{packageId}:{includePrerelease}` |
| Vulnerabilities | 6 hours | `vuln:{packageId}:{version}` |
| License Info | 24 hours | `license:{packageId}:{version}` |


## Project Structure

```
src/
├── NugetExplorer.Domain/    # Domain entities, interfaces, and services
└── NugetExplorer.Mcp/       # MCP server and tools
tests/
├── NugetExplorer.Tests/              # Unit tests
└── NugetExplorer.IntegrationTests/   # Integration tests
```

## Contributing

Contributions are welcome. This project follows domain-driven design principles with services in the Domain layer and MCP tool definitions in the Mcp layer.

## License

MIT
