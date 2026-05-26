using PerformantSitecore.Foundation.ManagedCache.Caching;

using Sitecore.Data.Items;

namespace PerformantSitecore.Foundation.ManagedCache.Services;

public delegate bool ManagedCacheInvalidationDelegate(IManagedCache cache, Item item, ItemEventType eventType, bool remote);