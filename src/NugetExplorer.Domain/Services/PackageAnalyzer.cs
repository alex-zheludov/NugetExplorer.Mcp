using Microsoft.Extensions.Logging;
using NuGetExplorerMcp.Application.Interfaces;
using NuGetExplorerMcp.Domain.Entities;
using NuGetExplorerMcp.Domain.Enums;
using NuGetExplorerMcp.Domain.ValueObjects;

namespace NuGetExplorerMcp.Application.Services;

/// <summary>
/// Main orchestrator for package analysis.
/// Coordinates update checking, vulnerability scanning, and license analysis.
/// Follows Single Responsibility Principle by delegating specific tasks to specialized services.
/// </summary>
public class PackageAnalyzer : IPackageAnalyzer
{
    private readonly ILogger<PackageAnalyzer> _logger;
    private readonly IUpdateChecker _updateChecker;
    private readonly IVulnerabilityScanner _vulnerabilityScanner;
    private readonly ILicenseAnalyzer _licenseAnalyzer;

    public PackageAnalyzer(
        ILogger<PackageAnalyzer> logger,
        IUpdateChecker updateChecker,
        IVulnerabilityScanner vulnerabilityScanner,
        ILicenseAnalyzer licenseAnalyzer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _updateChecker = updateChecker ?? throw new ArgumentNullException(nameof(updateChecker));
        _vulnerabilityScanner = vulnerabilityScanner ?? throw new ArgumentNullException(nameof(vulnerabilityScanner));
        _licenseAnalyzer = licenseAnalyzer ?? throw new ArgumentNullException(nameof(licenseAnalyzer));
    }

    public async Task<PackageAnalysisResult> AnalyzePackagesAsync(
        IEnumerable<PackageReference> packages,
        PackageAnalysisOptions options,
        CancellationToken cancellationToken = default)
    {
        var packageList = packages.Distinct().ToList();
        _logger.LogInformation("Starting analysis of {Count} packages", packageList.Count);

        // Analyze packages in parallel for better performance
        var analysisTasksBatched = packageList.Select(async package =>
        {
            try
            {
                return await AnalyzeSinglePackageAsync(package, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze package {PackageId}", package.Id);

                // Return a package analysis with error state
                return new PackageAnalysis
                {
                    Id = package.Id,
                    CurrentVersion = package.Version,
                    Updates = null,
                    Vulnerabilities = Array.Empty<Vulnerability>(),
                    License = null
                };
            }
        });

        var analysisResults = await Task.WhenAll(analysisTasksBatched);

        var summary = BuildSummary(analysisResults);

        return new PackageAnalysisResult
        {
            Summary = summary,
            Packages = analysisResults
        };
    }

    private async Task<PackageAnalysis> AnalyzeSinglePackageAsync(
        PackageReference package,
        PackageAnalysisOptions options,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Analyzing package {PackageId} {Version}", package.Id, package.Version);

        // Execute checks in parallel based on options
        var tasks = new List<Task>();
        Task<UpdateInfo?>? updateTask = null;
        Task<IReadOnlyList<Vulnerability>>? vulnTask = null;
        Task<LicenseChange?>? licenseTask = null;

        if (options.CheckUpdates)
        {
            updateTask = _updateChecker.CheckForUpdateAsync(package, options, cancellationToken);
            tasks.Add(updateTask);
        }

        if (options.CheckVulnerabilities)
        {
            vulnTask = _vulnerabilityScanner.GetVulnerabilitiesAsync(package, cancellationToken);
            tasks.Add(vulnTask);
        }

        await Task.WhenAll(tasks);

        var updateInfo = updateTask?.Result;

        // Only check license changes if there's an update available and license checking is enabled
        if (options.CheckLicenses && updateInfo != null)
        {
            licenseTask = _licenseAnalyzer.CheckLicenseChangeAsync(
                package,
                updateInfo.LatestStableVersion,
                cancellationToken);
        }

        var vulnerabilities = vulnTask?.Result ?? Array.Empty<Vulnerability>();
        var licenseChange = licenseTask != null ? await licenseTask : null;

        // Filter vulnerabilities by severity if specified
        if (options.MinimumSeverity > SeverityLevel.All)
        {
            vulnerabilities = vulnerabilities
                .Where(v => v.Severity >= options.MinimumSeverity)
                .ToList();
        }

        return new PackageAnalysis
        {
            Id = package.Id,
            CurrentVersion = package.Version,
            Updates = updateInfo,
            Vulnerabilities = vulnerabilities,
            License = licenseChange
        };
    }

    private AnalysisSummary BuildSummary(IReadOnlyList<PackageAnalysis> analyses)
    {
        var packagesWithUpdates = analyses.Count(a => a.Updates != null);
        var vulnerablePackages = analyses.Count(a => a.Vulnerabilities.Any());
        var packagesWithLicenseChanges = analyses.Count(a => a.License?.HasChanged == true);
        var upToDate = analyses.Count - packagesWithUpdates;

        var allVulnerabilities = analyses.SelectMany(a => a.Vulnerabilities).ToList();

        var severityCounts = new SeverityCounts
        {
            Critical = allVulnerabilities.Count(v => v.Severity == SeverityLevel.Critical),
            High = allVulnerabilities.Count(v => v.Severity == SeverityLevel.High),
            Medium = allVulnerabilities.Count(v => v.Severity == SeverityLevel.Medium),
            Low = allVulnerabilities.Count(v => v.Severity == SeverityLevel.Low)
        };

        return new AnalysisSummary
        {
            TotalPackages = analyses.Count,
            PackagesWithUpdates = packagesWithUpdates,
            VulnerablePackages = vulnerablePackages,
            PackagesWithLicenseChanges = packagesWithLicenseChanges,
            UpToDate = upToDate,
            SeverityCounts = severityCounts
        };
    }
}
