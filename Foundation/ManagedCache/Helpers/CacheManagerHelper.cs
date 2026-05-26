using System;
using System.Collections.Generic;
using System.Linq;

using PerformantSitecore.Foundation.ManagedCache.Caching;

using Sitecore.Caching;

namespace PerformantSitecore.Foundation.ManagedCache.Helpers;

public class CacheManagerHelper : ICacheManagerHelper
{
    public IEnumerable<string> GetAllCacheNames()
    {
        return CacheManager.GetAllCaches()
            .Select(c => c.Name);
    }

    public IManagedCache GetCache(string cacheName)
    {
        try
        {
            var cache = CacheManager.FindCacheByName<string>(cacheName);
            return cache == null ? null : new UnmanagedCacheAdapter(cache);
        }
        catch (Exception)
        {
            var nonGenericCache = CacheManager.GetAllCaches()
                .FirstOrDefault(cache => cache.Name == cacheName);
            return nonGenericCache == null ? null : new UnmanagedCacheAdapter(nonGenericCache);
        }
    }
}
