using NuGetExplorerMcp.Domain.Entities;
using NuGetExplorerMcp.Domain.ValueObjects;

namespace NuGetExplorerMcp.Application.Interfaces;

/// <summary>
/// Main service for analyzing NuGet packages.
/// Orchestrates update checking, vulnerability scanning, and license analysis.
/// </summary>
public interface IPackageAnalyzer
{
    /// <summary>
    /// Analyzes a collection of packages for updates, vulnerabilities, and license changes.
    /// </summary>
    /// <param name="packages">The packages to analyze.</param>
    /// <param name="options">Analysis options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete analysis results.</returns>
    Task<PackageAnalysisResult> AnalyzePackagesAsync(
        IEnumerable<PackageReference> packages,
        PackageAnalysisOptions options,
        CancellationToken cancellationToken = default);
}
