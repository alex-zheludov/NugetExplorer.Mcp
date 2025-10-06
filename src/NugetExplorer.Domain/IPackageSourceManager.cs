using NuGetExplorerMcp.Domain.Entities;

namespace NuGetExplorerMcp.Application.Interfaces;

/// <summary>
/// Manages NuGet package sources and retrieves package metadata.
/// </summary>
public interface IPackageSourceManager
{
    /// <summary>
    /// Gets all configured package sources from nuget.config.
    /// </summary>
    /// <returns>Collection of package sources.</returns>
    Task<IEnumerable<PackageSource>> GetConfiguredSourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available versions for a package from all sources.
    /// </summary>
    /// <param name="packageId">The package ID.</param>
    /// <param name="includePrerelease">Whether to include prerelease versions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available versions.</returns>
    Task<IReadOnlyList<string>> GetAllVersionsAsync(
        string packageId,
        bool includePrerelease,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the license information for a specific package version.
    /// </summary>
    /// <param name="packageId">The package ID.</param>
    /// <param name="version">The package version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>License expression or URL.</returns>
    Task<string?> GetPackageLicenseAsync(
        string packageId,
        string version,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the project/repository URL for a package.
    /// </summary>
    /// <param name="packageId">The package ID.</param>
    /// <param name="version">The package version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Project URL if available.</returns>
    Task<string?> GetProjectUrlAsync(
        string packageId,
        string version,
        CancellationToken cancellationToken = default);
}
