using Microsoft.Extensions.Logging;
using NuGetExplorerMcp.Application.Interfaces;
using NuGetExplorerMcp.Domain.Entities;
using NuGetExplorerMcp.Domain.Enums;
using NuGetExplorerMcp.Domain.ValueObjects;

namespace NuGetExplorerMcp.Infrastructure.Services;

/// <summary>
/// Analyzes package licenses and detects license changes between versions.
/// </summary>
public class LicenseAnalyzer : ILicenseAnalyzer
{
    private readonly ILogger<LicenseAnalyzer> _logger;
    private readonly IPackageSourceManager _packageSourceManager;

    // Common open-source licenses
    private static readonly HashSet<string> PermissiveLicenses = new(StringComparer.OrdinalIgnoreCase)
    {
        "MIT", "Apache-2.0", "BSD-2-Clause", "BSD-3-Clause", "ISC", "0BSD"
    };

    private static readonly HashSet<string> CopyleftLicenses = new(StringComparer.OrdinalIgnoreCase)
    {
        "GPL-2.0", "GPL-3.0", "AGPL-3.0", "LGPL-2.1", "LGPL-3.0"
    };

    private static readonly HashSet<string> ProprietaryKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "commercial", "proprietary", "closed-source", "closed source"
    };

    public LicenseAnalyzer(
        ILogger<LicenseAnalyzer> logger,
        IPackageSourceManager packageSourceManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _packageSourceManager = packageSourceManager ?? throw new ArgumentNullException(nameof(packageSourceManager));
    }

    public async Task<LicenseChange?> CheckLicenseChangeAsync(
        PackageReference package,
        string latestVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentLicense = await _packageSourceManager.GetPackageLicenseAsync(
                package.Id,
                package.Version,
                cancellationToken);

            var latestLicense = await _packageSourceManager.GetPackageLicenseAsync(
                package.Id,
                latestVersion,
                cancellationToken);

            // Normalize licenses for comparison
            var normalizedCurrent = NormalizeLicense(currentLicense);
            var normalizedLatest = NormalizeLicense(latestLicense);

            if (string.Equals(normalizedCurrent, normalizedLatest, StringComparison.OrdinalIgnoreCase))
            {
                return null; // No change
            }

            var severity = DetermineLicenseChangeSeverity(normalizedCurrent, normalizedLatest);
            var description = BuildChangeDescription(normalizedCurrent, normalizedLatest, severity);

            return new LicenseChange
            {
                CurrentLicense = normalizedCurrent ?? "Unknown",
                LatestLicense = normalizedLatest ?? "Unknown",
                HasChanged = true,
                Severity = severity,
                Description = description,
                CurrentLicenseUrl = IsUrl(currentLicense) ? currentLicense : null,
                LatestLicenseUrl = IsUrl(latestLicense) ? latestLicense : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check license change for package {PackageId}", package.Id);
            return null;
        }
    }

    private string? NormalizeLicense(string? license)
    {
        if (string.IsNullOrWhiteSpace(license))
            return null;

        // If it's a URL, try to extract license type from it
        if (IsUrl(license))
        {
            if (license.Contains("MIT", StringComparison.OrdinalIgnoreCase))
                return "MIT";
            if (license.Contains("Apache", StringComparison.OrdinalIgnoreCase))
                return "Apache-2.0";
            if (license.Contains("GPL", StringComparison.OrdinalIgnoreCase))
                return "GPL";

            return "Custom License (see URL)";
        }

        // Clean up common variations
        license = license.Trim();
        if (license.Equals("MIT License", StringComparison.OrdinalIgnoreCase))
            return "MIT";
        if (license.Equals("Apache License 2.0", StringComparison.OrdinalIgnoreCase))
            return "Apache-2.0";

        return license;
    }

    private SeverityLevel DetermineLicenseChangeSeverity(string? current, string? latest)
    {
        // No license to having a license is informational
        if (string.IsNullOrEmpty(current) && !string.IsNullOrEmpty(latest))
            return SeverityLevel.Low;

        // Having a license to no license is concerning
        if (!string.IsNullOrEmpty(current) && string.IsNullOrEmpty(latest))
            return SeverityLevel.Medium;

        // Check for proprietary/commercial change
        if (IsProprietaryLicense(latest) && !IsProprietaryLicense(current))
            return SeverityLevel.Critical;

        // Check for copyleft change
        if (IsCopyleftLicense(latest) && IsPermissiveLicense(current))
            return SeverityLevel.High;

        // Check for permissive to more restrictive
        if (IsPermissiveLicense(current) && !IsPermissiveLicense(latest))
            return SeverityLevel.High;

        // Any other license change
        return SeverityLevel.Medium;
    }

    private string BuildChangeDescription(string? current, string? latest, SeverityLevel severity)
    {
        return severity switch
        {
            SeverityLevel.Critical => $"Package changed from {current} to commercial/proprietary license",
            SeverityLevel.High => $"Package license changed from {current} to {latest} (more restrictive)",
            SeverityLevel.Medium => $"Package license changed from {current} to {latest}",
            SeverityLevel.Low => "Package now includes license information (improvement)",
            _ => $"License changed from {current} to {latest}"
        };
    }

    private bool IsUrl(string? license)
    {
        return !string.IsNullOrEmpty(license) &&
               (license.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                license.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
    }

    private bool IsPermissiveLicense(string? license)
    {
        return !string.IsNullOrEmpty(license) &&
               PermissiveLicenses.Any(l => license.Contains(l, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsCopyleftLicense(string? license)
    {
        return !string.IsNullOrEmpty(license) &&
               CopyleftLicenses.Any(l => license.Contains(l, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsProprietaryLicense(string? license)
    {
        return !string.IsNullOrEmpty(license) &&
               ProprietaryKeywords.Any(k => license.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}
