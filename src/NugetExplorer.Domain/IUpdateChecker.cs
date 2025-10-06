using NuGetExplorerMcp.Domain.Entities;
using NuGetExplorerMcp.Domain.ValueObjects;

namespace NuGetExplorerMcp.Application.Interfaces;

/// <summary>
/// Checks for package updates and determines version compatibility.
/// </summary>
public interface IUpdateChecker
{
    /// <summary>
    /// Checks if a newer version is available for the package.
    /// </summary>
    /// <param name="package">The package to check.</param>
    /// <param name="options">Analysis options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Update information if a newer version exists, null otherwise.</returns>
    Task<UpdateInfo?> CheckForUpdateAsync(
        PackageReference package,
        PackageAnalysisOptions options,
        CancellationToken cancellationToken = default);
}
