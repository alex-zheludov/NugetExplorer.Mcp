namespace NuGetExplorerMcp.Application.Interfaces;

/// <summary>
/// Caching service to optimize repeated package queries.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value or creates it using the provided factory.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory function to create the value if not cached.</param>
    /// <param name="ttl">Time-to-live for the cached value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);
}
