using NuGetExplorerMcp.Domain.Enums;

namespace NuGetExplorerMcp.Domain.Entities;

/// <summary>
/// Information about available package updates.
/// </summary>
public class UpdateInfo
{
    public required string LatestStableVersion { get; init; }
    public string? LatestPrereleaseVersion { get; init; }
    public required VersionChangeType VersionChangeType { get; init; }
    public required bool IsCompatible { get; init; }
    public string? ReleaseNotesUrl { get; init; }
}
