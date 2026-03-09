using System.Collections.Concurrent;

namespace PerformantSitecore.Foundation.SqlDataProvider.Caching;

/// <summary>
/// Manages one ParentChildRelationCache instance per Sitecore database.
/// Thread-safe via ConcurrentDictionary.
/// </summary>
public class DatabaseCacheMap(string cacheNamePrefix, long maxSize)
{
    private readonly ConcurrentDictionary<string, ParentChildRelationCache> _caches
        = new ConcurrentDictionary<string, ParentChildRelationCache>();

    public ParentChildRelationCache GetCacheForDatabase(string databaseName)
    {
        return _caches.GetOrAdd(databaseName,
            name => new ParentChildRelationCache($"{cacheNamePrefix}[{name}]", maxSize));
    }
}