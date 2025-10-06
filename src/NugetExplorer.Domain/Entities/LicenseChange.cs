using NuGetExplorerMcp.Domain.Enums;

namespace NuGetExplorerMcp.Domain.Entities;

/// <summary>
/// Represents a license change between package versions.
/// </summary>
public class LicenseChange
{
    public required string CurrentLicense { get; init; }
    public required string LatestLicense { get; init; }
    public required bool HasChanged { get; init; }
    public required SeverityLevel Severity { get; init; }
    public required string Description { get; init; }
    public string? CurrentLicenseUrl { get; init; }
    public string? LatestLicenseUrl { get; init; }
}
