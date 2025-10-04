using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using NuGetExplorerMcp.Application.Interfaces;
using NuGetExplorerMcp.Domain.Enums;
using NuGetExplorerMcp.Domain.ValueObjects;
using NuGetExplorerMcp.Server.Models;

namespace NuGetExplorerMcp.Server.Tools;

/// <summary>
/// MCP tools for NuGet package analysis and management.
/// </summary>
[McpServerToolType]
public static class NuGetTools
{
    [McpServerTool(Name = "analyze_packages")]
    [Description("Comprehensive analysis of NuGet packages. Checks for updates, security vulnerabilities, and license changes.")]
    public static async Task<string> AnalyzePackages(
        IPackageAnalyzer packageAnalyzer,
        [Description("Array of packages to analyze")] PackageInput[] packages,
        [Description("Target framework (e.g., net8.0)")] string? targetFramework = null,
        [Description("Include prerelease versions")] bool includePrerelease = false,
        [Description("Check for package updates")] bool checkUpdates = true,
        [Description("Check for security vulnerabilities")] bool checkVulnerabilities = true,
        [Description("Check for license changes")] bool checkLicenses = true,
        [Description("Filter vulnerabilities by severity (all, low, medium, high, critical)")] string severityFilter = "all",
        CancellationToken cancellationToken = default)
    {
        var packageReferences = packages
            .Select(p => new PackageReference { Id = p.Id, Version = p.Version })
            .ToList();

        var options = new PackageAnalysisOptions
        {
            TargetFramework = targetFramework,
            IncludePrerelease = includePrerelease,
            CheckUpdates = checkUpdates,
            CheckVulnerabilities = checkVulnerabilities,
            CheckLicenses = checkLicenses,
            MinimumSeverity = ParseSeverityFilter(severityFilter)
        };

        var result = await packageAnalyzer.AnalyzePackagesAsync(
            packageReferences,
            options,
            cancellationToken);

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    [McpServerTool(Name = "list_package_sources")]
    [Description("List configured NuGet package sources from nuget.config")]
    public static async Task<string> ListPackageSources(
        IPackageSourceManager packageSourceManager,
        CancellationToken cancellationToken = default)
    {
        var sources = await packageSourceManager.GetConfiguredSourcesAsync(cancellationToken);

        var result = new
        {
            sources = sources.Select(s => new
            {
                name = s.Name,
                url = s.Url,
                isEnabled = s.IsEnabled,
                isOfficial = s.IsOfficial,
                requiresAuth = s.RequiresAuth,
                isAuthenticated = s.IsAuthenticated
            })
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static SeverityLevel ParseSeverityFilter(string filter)
    {
        return filter?.ToLowerInvariant() switch
        {
            "critical" => SeverityLevel.Critical,
            "high" => SeverityLevel.High,
            "medium" => SeverityLevel.Medium,
            "low" => SeverityLevel.Low,
            _ => SeverityLevel.All
        };
    }
}
