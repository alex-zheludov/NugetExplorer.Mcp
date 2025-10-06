namespace NuGetExplorerMcp.Domain.ValueObjects;

/// <summary>
/// Represents a reference to a NuGet package with its ID and version.
/// </summary>
public record PackageReference
{
    public required string Id { get; init; }
    public required string Version { get; init; }

    public override string ToString() => $"{Id} {Version}";
}
