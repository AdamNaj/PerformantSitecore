using System.Collections.Generic;

using PerformantSitecore.Foundation.ManagedCache.Caching;

using Sitecore.Data;
using Sitecore.Data.Items;

namespace PerformantSitecore.Foundation.ManagedCache.Services;

public interface IManagedCacheService
{
    IManagedCache GetCache(string cacheName, string defaultMaxSize = "50MB",
        ManagedCacheInvalidationDelegate clearer = null, ID[] invalidatingTemplateIds = null);

    bool ClearCaches(Item item, ItemEventType eventType, bool remote);

    List<string> GetCacheNames(bool includeUnmanaged = false);

    IManagedCache GetCacheByName(string cacheName, bool includeUnmanaged = false);

    void ClearCache(string cacheName, bool includeUnmanaged = false);

    void RaiseClearCacheEvent(string cacheName, string userName);

    void RaiseClearCacheEventOnRemotes(string cacheName, string userName);

    bool IsManagedCache(IManagedCache cache);
}