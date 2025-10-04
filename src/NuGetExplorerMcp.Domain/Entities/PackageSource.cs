namespace NuGetExplorerMcp.Domain.Entities;

/// <summary>
/// Represents a NuGet package source configuration.
/// </summary>
public class PackageSource
{
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required bool IsEnabled { get; init; }
    public required bool IsOfficial { get; init; }
    public bool RequiresAuth { get; init; }
    public bool IsAuthenticated { get; init; }
}
