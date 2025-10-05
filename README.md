# NuGet Explorer MCP Server

A Model Context Protocol (MCP) server that provides comprehensive NuGet package analysis capabilities. Check packages for updates, security vulnerabilities, and license changes - all from your AI coding assistant.

## Features

- **Update Detection** - Check if newer package versions exist (stable & prerelease)
- **Vulnerability Scanning** - Scan for known CVEs via GitHub Advisory Database
- **License Change Detection** - Alert when licenses change between versions
- **Private Feed Support** - Works with nuget.config (Azure Artifacts, GitHub Packages, etc.)
- **Intelligent Caching** - Optimized for performance with configurable TTLs
- **Clean Architecture** - SOLID principles, dependency injection, separation of concerns

## Architecture

Built using the [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) with Clean Architecture and SOLID principles:

```
┌─────────────────────────────────────────────────────┐
│ MCP Server Layer                                     │
│ - Attribute-based Tools ([McpServerTool])           │
│ - Dependency Injection via SDK                      │
│ - STDIO Transport                                   │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│ Application Layer                                    │
│ - PackageAnalyzer (Orchestrator)                    │
│ - Business Logic & Use Cases                        │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│ Infrastructure Layer                                 │
│ - PackageSourceManager (NuGet API)                  │
│ - UpdateChecker (Version Comparison)                │
│ - VulnerabilityScanner (GitHub Advisory)            │
│ - LicenseAnalyzer (License Detection)               │
│ - MemoryCacheService (Performance)                  │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│ Domain Layer                                         │
│ - Entities (PackageAnalysis, Vulnerability, etc.)   │
│ - Value Objects (PackageReference, Options)         │
│ - Enums (SeverityLevel, VersionChangeType)          │
└─────────────────────────────────────────────────────┘
```

### SOLID Principles Applied

- **Single Responsibility**: Each service has one focused responsibility
- **Open/Closed**: Extensible without modifying existing code
- **Liskov Substitution**: Interfaces can be swapped with implementations
- **Interface Segregation**: Focused interfaces for each concern
- **Dependency Inversion**: Depends on abstractions, not concretions

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

## Quick Start

### 1. Build the Server

```bash
dotnet build NuGetExplorerMcp.sln -c Release
```

### 2. Setup with Your AI Assistant

#### **Claude Code (VS Code)**

1. Install the [Claude Code extension](https://marketplace.visualstudio.com/items?itemName=Anthropic.claude-code) from VS Code marketplace

2. Open Claude Code in VS Code and run:

**Windows:**
```bash
claude mcp add-json nuget-explorer '{"type":"stdio","command":"cmd","args":["/c","dotnet run --project [PATH TO REPOS]/Nuget.Mcp/src/NuGetExplorerMcp.Server/NuGetExplorerMcp.Server.csproj --no-build -c Release"],"env":{}}' --scope user
```

**Mac/Linux:**
```bash
claude mcp add nuget-explorer -- dotnet run --project ~/Code/Personal/Nuget.Mcp/src/NuGetExplorerMcp.Server/NuGetExplorerMcp.Server.csproj --no-build -c Release
```

   **Note:** Update the path to match where you cloned this repository.

3. Reload VS Code (Ctrl+Shift+P → "Developer: Reload Window")

4. Open Claude Code and ask: *"Check my NuGet packages for updates and vulnerabilities"*

#### **GitHub Copilot (VS Code)**

1. Install the [GitHub Copilot extension](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) from VS Code marketplace

2. **Option A: Repository-specific** - Create `.vscode/mcp.json` in your project:

```json
{
  "inputs": [],
  "servers": {
    "nuget-explorer": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:/Code/Personal/Nuget.Mcp/src/NuGetExplorerMcp.Server/NuGetExplorerMcp.Server.csproj",
        "--no-build",
        "-c",
        "Release"
      ]
    }
  }
}
```

   **OR**

   **Option B: Global** - Add to your VS Code `settings.json`:

```json
{
  "github.copilot.chat.mcp.servers": {
    "nuget-explorer": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:/Code/Personal/Nuget.Mcp/src/NuGetExplorerMcp.Server/NuGetExplorerMcp.Server.csproj",
        "--no-build",
        "-c",
        "Release"
      ]
    }
  }
}
```

   **Note:** Update the path to match where you cloned this repository. Use forward slashes `/` even on Windows.

3. Reload VS Code or click "Start" next to the server name in Copilot Chat

4. Verify the server is running by checking the tools icon in Copilot Chat

5. Ask Copilot: *"Check my NuGet packages for updates and vulnerabilities"*

📖 [GitHub Copilot MCP Documentation](https://docs.github.com/en/copilot/how-tos/provide-context/use-mcp/extend-copilot-chat-with-mcp)

### 3. Test the Installation

**Windows:**
```powershell
.\tests\integration\quick-test.ps1
# or
.\tests\integration\test-mcp-server.ps1
```

## Usage Examples

Once configured with Claude Code, you can ask natural language questions:

### Check Packages for Updates
```
"Check if Newtonsoft.Json 12.0.0 has any updates available"
```

### Security Scan
```
"Scan my packages for security vulnerabilities: Newtonsoft.Json 12.0.0 and Serilog 2.10.0"
```

### Full Analysis
```
"Analyze these packages for updates, vulnerabilities, and license changes:
- Newtonsoft.Json 12.0.0
- Serilog 2.10.0
- EntityFrameworkCore 6.0.0"
```

### List Package Sources
```
"What NuGet sources are configured on my machine?"
```

The server will automatically parse your request and return structured analysis with:
- Available updates (stable & prerelease versions)
- Security vulnerabilities with severity levels
- License changes between versions
- Compatibility information

## Project Structure

```
NuGetExplorerMcp/
├── src/
│   ├── NuGetExplorerMcp.Domain/          # Domain entities & value objects
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── ValueObjects/
│   ├── NuGetExplorerMcp.Application/     # Business logic & interfaces
│   │   ├── Interfaces/
│   │   └── Services/
│   ├── NuGetExplorerMcp.Infrastructure/  # External integrations
│   │   └── Services/
│   └── NuGetExplorerMcp.Server/         # MCP server entry point
│       ├── Tools/
│       ├── Models/
│       ├── ServiceConfiguration.cs
│       └── Program.cs
└── tests/
    └── NuGetExplorerMcp.Tests/
```

## Caching Strategy

Optimized for performance with in-memory caching:

| Data Type | TTL | Cache Key Pattern |
|-----------|-----|-------------------|
| Package Metadata | 1 hour | `pkg:{packageId}:{version}` |
| All Versions | 1 hour | `versions:{packageId}:{includePrerelease}` |
| Vulnerabilities | 6 hours | `vuln:{packageId}:{version}` |
| License Info | 24 hours | `license:{packageId}:{version}` |

## Dependencies

### NuGet Packages
- `NuGet.Protocol` - NuGet V3 API client
- `NuGet.Versioning` - SemVer version comparison
- `NuGet.Configuration` - nuget.config parsing
- `NuGet.Packaging` - Package metadata parsing
- `Microsoft.Extensions.Caching.Memory` - In-memory caching
- `Microsoft.Extensions.DependencyInjection` - DI container
- `Microsoft.Extensions.Logging` - Structured logging

## Performance

- **Single package analysis**: < 1 second
- **Batch (10 packages)**: < 4 seconds
- **Batch (50 packages)**: < 15 seconds

Parallel processing is used to query multiple packages simultaneously.

## Contributing

This server follows Clean Architecture principles. When adding features:

1. **Domain Layer**: Add entities/value objects
2. **Application Layer**: Define interfaces
3. **Infrastructure Layer**: Implement services
4. **Server Layer**: Expose via MCP tools

Maintain SOLID principles and add appropriate logging.

## License

MIT
