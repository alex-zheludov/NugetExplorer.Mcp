using NuGetExplorerMcp.Domain.Entities;
using NuGetExplorerMcp.Domain.ValueObjects;

namespace NuGetExplorerMcp.Application.Interfaces;

/// <summary>
/// Analyzes package licenses and detects license changes between versions.
/// </summary>
public interface ILicenseAnalyzer
{
    /// <summary>
    /// Checks for license changes between the current and latest versions.
    /// </summary>
    /// <param name="package">The package to check.</param>
    /// <param name="latestVersion">The latest version to compare against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>License change information if licenses differ, null otherwise.</returns>
    Task<LicenseChange?> CheckLicenseChangeAsync(
        PackageReference package,
        string latestVersion,
        CancellationToken cancellationToken = default);
}
