using NuGetExplorerMcp.Domain.Enums;

namespace NuGetExplorerMcp.Domain.Entities;

/// <summary>
/// Complete analysis result for multiple packages.
/// </summary>
public class PackageAnalysisResult
{
    public required AnalysisSummary Summary { get; init; }
    public required IReadOnlyList<PackageAnalysis> Packages { get; init; }
}

/// <summary>
/// Summary statistics for the package analysis.
/// </summary>
public class AnalysisSummary
{
    public required int TotalPackages { get; init; }
    public required int PackagesWithUpdates { get; init; }
    public required int VulnerablePackages { get; init; }
    public required int PackagesWithLicenseChanges { get; init; }
    public required int UpToDate { get; init; }
    public required SeverityCounts SeverityCounts { get; init; }
}

/// <summary>
/// Count of vulnerabilities by severity level.
/// </summary>
public class SeverityCounts
{
    public int Critical { get; init; }
    public int High { get; init; }
    public int Medium { get; init; }
    public int Low { get; init; }
}
