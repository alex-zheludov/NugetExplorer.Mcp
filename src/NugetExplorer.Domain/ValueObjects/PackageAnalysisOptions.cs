using NuGetExplorerMcp.Domain.Enums;

namespace NuGetExplorerMcp.Domain.ValueObjects;

/// <summary>
/// Options for configuring package analysis behavior.
/// </summary>
public class PackageAnalysisOptions
{
    public string? TargetFramework { get; init; }
    public bool IncludePrerelease { get; init; }
    public bool CheckUpdates { get; init; } = true;
    public bool CheckVulnerabilities { get; init; } = true;
    public bool CheckLicenses { get; init; } = true;
    public SeverityLevel MinimumSeverity { get; init; } = SeverityLevel.All;
}
