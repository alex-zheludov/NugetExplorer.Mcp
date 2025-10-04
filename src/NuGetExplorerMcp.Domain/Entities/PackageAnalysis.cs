namespace NuGetExplorerMcp.Domain.Entities;

/// <summary>
/// Complete analysis result for a single package.
/// </summary>
public class PackageAnalysis
{
    public required string Id { get; init; }
    public required string CurrentVersion { get; init; }
    public UpdateInfo? Updates { get; init; }
    public IReadOnlyList<Vulnerability> Vulnerabilities { get; init; } = Array.Empty<Vulnerability>();
    public LicenseChange? License { get; init; }
}
