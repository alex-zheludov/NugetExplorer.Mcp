namespace NuGetExplorerMcp.Domain.Enums;

/// <summary>
/// Type of version change based on SemVer.
/// </summary>
public enum VersionChangeType
{
    None = 0,
    Patch = 1,    // 1.0.0 → 1.0.1 (safe)
    Minor = 2,    // 1.0.0 → 1.1.0 (should be safe)
    Major = 3     // 1.0.0 → 2.0.0 (breaking changes possible)
}
