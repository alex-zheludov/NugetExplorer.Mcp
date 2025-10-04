using Microsoft.Extensions.Caching.Memory;
using NuGetExplorerMcp.Application.Interfaces;

namespace NuGetExplorerMcp.Infrastructure.Services;

/// <summary>
/// In-memory caching implementation to optimize repeated package queries.
/// Cache keys follow the pattern: "type:packageId:version" for efficient lookups.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory(cancellationToken);

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        _cache.Set(key, value, cacheEntryOptions);

        return value;
    }
}
