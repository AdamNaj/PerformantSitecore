using System.Collections.Generic;

using PerformantSitecore.Foundation.ManagedCache.Caching;

namespace PerformantSitecore.Foundation.ManagedCache.Helpers;

public interface ICacheManagerHelper
{
    IEnumerable<string> GetAllCacheNames();

    IManagedCache GetCache(string cacheName);
}
