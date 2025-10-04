namespace NuGetExplorerMcp.Server.Models;

/// <summary>
/// Input model for package analysis.
/// </summary>
public class PackageInput
{
    public required string Id { get; init; }
    public required string Version { get; init; }
}
