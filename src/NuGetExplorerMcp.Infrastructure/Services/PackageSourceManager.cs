using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetExplorerMcp.Application.Interfaces;
using NuGetExplorerMcp.Domain.Entities;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using NuGetPackageSource = NuGet.Configuration.PackageSource;
using DomainPackageSource = NuGetExplorerMcp.Domain.Entities.PackageSource;

namespace NuGetExplorerMcp.Infrastructure.Services;

/// <summary>
/// Manages NuGet package sources and retrieves package metadata from configured feeds.
/// Implements Single Responsibility Principle (SRP) by focusing only on package source management.
/// </summary>
public class PackageSourceManager : IPackageSourceManager
{
    private readonly ILogger<PackageSourceManager> _logger;
    private readonly ICacheService _cacheService;
    private readonly ISettings _nugetSettings;
    private readonly SourceCacheContext _cacheContext;

    public PackageSourceManager(
        ILogger<PackageSourceManager> logger,
        ICacheService cacheService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

        // Load NuGet.config from default locations (user/machine-wide)
        _nugetSettings = Settings.LoadDefaultSettings(root: null);
        _cacheContext = new SourceCacheContext();
    }

    public async Task<IEnumerable<DomainPackageSource>> GetConfiguredSourcesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrSetAsync(
            "sources:configured",
            async ct =>
            {
                var packageSourceProvider = new PackageSourceProvider(_nugetSettings);
                var sources = packageSourceProvider.LoadPackageSources()
                    .Where(s => s.IsEnabled)
                    .Select(s => new DomainPackageSource
                    {
                        Name = s.Name,
                        Url = s.Source,
                        IsEnabled = s.IsEnabled,
                        IsOfficial = s.Source.Contains("nuget.org", StringComparison.OrdinalIgnoreCase),
                        RequiresAuth = !string.IsNullOrEmpty(s.Credentials?.Username),
                        IsAuthenticated = !string.IsNullOrEmpty(s.Credentials?.Username)
                    })
                    .ToList();

                _logger.LogInformation("Loaded {Count} enabled package sources", sources.Count);
                return (IEnumerable<DomainPackageSource>)sources;
            },
            TimeSpan.FromHours(1),
            cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAllVersionsAsync(
        string packageId,
        bool includePrerelease,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"versions:{packageId}:{includePrerelease}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                var versions = new HashSet<NuGetVersion>();
                var sources = await GetSourceRepositoriesAsync(ct);

                foreach (var source in sources)
                {
                    try
                    {
                        var findPackageByIdResource = await source.GetResourceAsync<FindPackageByIdResource>(ct);
                        var packageVersions = await findPackageByIdResource.GetAllVersionsAsync(
                            packageId,
                            _cacheContext,
                            new NullLogger(),
                            ct);

                        foreach (var version in packageVersions)
                        {
                            if (includePrerelease || !version.IsPrerelease)
                            {
                                versions.Add(version);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to query package {PackageId} from source {Source}",
                            packageId, source.PackageSource.Name);
                    }
                }

                return versions
                    .OrderByDescending(v => v)
                    .Select(v => v.ToString())
                    .ToList() as IReadOnlyList<string>;
            },
            TimeSpan.FromHours(1),
            cancellationToken);
    }

    public async Task<string?> GetPackageLicenseAsync(
        string packageId,
        string version,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"license:{packageId}:{version}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                var metadata = await GetPackageMetadataAsync(packageId, version, ct);
                return metadata?.LicenseMetadata?.License ?? metadata?.LicenseUrl?.ToString();
            },
            TimeSpan.FromHours(24),
            cancellationToken);
    }

    public async Task<string?> GetProjectUrlAsync(
        string packageId,
        string version,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"projecturl:{packageId}:{version}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                var metadata = await GetPackageMetadataAsync(packageId, version, ct);
                return metadata?.ProjectUrl?.ToString();
            },
            TimeSpan.FromHours(24),
            cancellationToken);
    }

    private async Task<IPackageSearchMetadata?> GetPackageMetadataAsync(
        string packageId,
        string version,
        CancellationToken cancellationToken)
    {
        var sources = await GetSourceRepositoriesAsync(cancellationToken);

        foreach (var source in sources)
        {
            try
            {
                var metadataResource = await source.GetResourceAsync<PackageMetadataResource>(cancellationToken);
                var nugetVersion = NuGetVersion.Parse(version);

                var metadata = await metadataResource.GetMetadataAsync(
                    packageId,
                    includePrerelease: true,
                    includeUnlisted: false,
                    _cacheContext,
                    new NullLogger(),
                    cancellationToken);

                var specificMetadata = metadata.FirstOrDefault(m => m.Identity.Version == nugetVersion);
                if (specificMetadata != null)
                {
                    return specificMetadata;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get metadata for {PackageId} {Version} from {Source}",
                    packageId, version, source.PackageSource.Name);
            }
        }

        return null;
    }

    private async Task<List<SourceRepository>> GetSourceRepositoriesAsync(CancellationToken cancellationToken)
    {
        // Get NuGet PackageSources directly from settings to preserve their configuration
        var packageSourceProvider = new PackageSourceProvider(_nugetSettings);
        var enabledSources = packageSourceProvider.LoadPackageSources()
            .Where(s => s.IsEnabled)
            .ToList();

        var providers = Repository.Provider.GetCoreV3();

        return enabledSources
            .Select(s => new SourceRepository(s, providers))
            .ToList();
    }
}
