using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using NuGetExplorerMcp.Application.Interfaces;
using NuGetExplorerMcp.Domain.Entities;
using NuGetExplorerMcp.Domain.Enums;
using NuGetExplorerMcp.Domain.ValueObjects;

namespace NuGetExplorerMcp.Infrastructure.Services;

/// <summary>
/// Checks for package updates and determines version compatibility.
/// Follows Open/Closed Principle (OCP) - can be extended for custom version comparison logic.
/// </summary>
public class UpdateChecker : IUpdateChecker
{
    private readonly ILogger<UpdateChecker> _logger;
    private readonly IPackageSourceManager _packageSourceManager;

    public UpdateChecker(
        ILogger<UpdateChecker> logger,
        IPackageSourceManager packageSourceManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _packageSourceManager = packageSourceManager ?? throw new ArgumentNullException(nameof(packageSourceManager));
    }

    public async Task<UpdateInfo?> CheckForUpdateAsync(
        PackageReference package,
        PackageAnalysisOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentVersion = NuGetVersion.Parse(package.Version);
            var allVersions = await _packageSourceManager.GetAllVersionsAsync(
                package.Id,
                options.IncludePrerelease,
                cancellationToken);

            if (!allVersions.Any())
            {
                _logger.LogWarning("No versions found for package {PackageId}", package.Id);
                return null;
            }

            var parsedVersions = allVersions
                .Select(v => NuGetVersion.Parse(v))
                .OrderByDescending(v => v)
                .ToList();

            var latestStable = parsedVersions.FirstOrDefault(v => !v.IsPrerelease);
            var latestPrerelease = options.IncludePrerelease
                ? parsedVersions.FirstOrDefault(v => v.IsPrerelease)
                : null;

            var latestVersion = latestStable ?? parsedVersions.First();

            // Check if current version is already the latest
            if (currentVersion >= latestVersion)
            {
                return null;
            }

            var versionChangeType = DetermineVersionChangeType(currentVersion, latestVersion);
            var projectUrl = await _packageSourceManager.GetProjectUrlAsync(
                package.Id,
                latestVersion.ToString(),
                cancellationToken);

            return new UpdateInfo
            {
                LatestStableVersion = latestStable?.ToString() ?? latestVersion.ToString(),
                LatestPrereleaseVersion = latestPrerelease?.ToString(),
                VersionChangeType = versionChangeType,
                ReleaseDate = null, // Would require additional API calls to determine
                IsCompatible = await CheckFrameworkCompatibilityAsync(
                    package.Id,
                    latestVersion.ToString(),
                    options.TargetFramework,
                    cancellationToken),
                ReleaseNotesUrl = BuildReleaseNotesUrl(projectUrl, latestVersion.ToString())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for updates for package {PackageId}", package.Id);
            return null;
        }
    }

    private VersionChangeType DetermineVersionChangeType(NuGetVersion current, NuGetVersion latest)
    {
        if (latest.Major > current.Major)
            return VersionChangeType.Major;

        if (latest.Minor > current.Minor)
            return VersionChangeType.Minor;

        if (latest.Patch > current.Patch)
            return VersionChangeType.Patch;

        return VersionChangeType.None;
    }

    private async Task<bool> CheckFrameworkCompatibilityAsync(
        string packageId,
        string version,
        string? targetFramework,
        CancellationToken cancellationToken)
    {
        // TODO: Implement a real TFM compatibility check by reading the package's supported target frameworks
        // from NuGet metadata (e.g., package registration/flat container nuspec) and comparing with targetFramework.
        return true;
    }

    private string? BuildReleaseNotesUrl(string? projectUrl, string version)
    {
        if (string.IsNullOrEmpty(projectUrl))
            return null;

        // Attempt to build GitHub release URL if it's a GitHub project
        if (projectUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase))
        {
            var cleanUrl = projectUrl.TrimEnd('/');
            return $"{cleanUrl}/releases/tag/v{version}";
        }

        return projectUrl;
    }
}
