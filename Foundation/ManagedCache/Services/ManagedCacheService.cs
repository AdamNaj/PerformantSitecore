using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PerformantSitecore.Foundation.ManagedCache.Caching;
using PerformantSitecore.Foundation.ManagedCache.EventHandlers;
using PerformantSitecore.Foundation.ManagedCache.Helpers;

using Sitecore;
using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Eventing;

namespace PerformantSitecore.Foundation.ManagedCache.Services;

public class ManagedCacheService(BaseEventQueueProvider provider, ICacheManagerHelper cacheManagerHelper)
    : IManagedCacheService
{
    private readonly ConcurrentDictionary<string, IManagedCache> _managedCaches =
        new ConcurrentDictionary<string, IManagedCache>();

    private readonly bool _cacheEnabled = Settings.GetBoolSetting(
        "PerformantSitecore.ManagedCache.Enabled", true);
    private readonly bool _raiseCacheClearEventsGlobally = Settings.GetBoolSetting(
        "PerformantSitecore.ManagedCache.RaiseClearEventsGlobally", true);
    private readonly bool _raiseCacheClearEventsLocally = Settings.GetBoolSetting(
        "PerformantSitecore.ManagedCache.RaiseClearEventsLocally", true);
    private readonly IEventQueue _queue = provider;
    private readonly ICacheManagerHelper _cacheManagerHelper = cacheManagerHelper;

    public IManagedCache GetCache(string cacheName, string defaultMaxSize = "50MB",
        ManagedCacheInvalidationDelegate clearer = null, ID[] invalidatingTemplateIds = null)
    {
        if (!_cacheEnabled)
            return null;

        if (_managedCaches.TryGetValue(cacheName, out var cache))
        {
            return cache;
        }

        long cacheMaxSize = StringUtil.ParseSizeString(
            Settings.GetSetting($"{cacheName}MaxSize", defaultMaxSize));
        bool cacheEnabled = Settings.GetBoolSetting($"{cacheName}CacheEnabled", true);

        if (!cacheEnabled)
            return null;

        cache = _managedCaches.GetOrAdd(cacheName,
            new Caching.ManagedCache(cacheName, cacheMaxSize, clearer));

        if (invalidatingTemplateIds != null)
        {
            foreach (var templateId in invalidatingTemplateIds)
            {
                cache.AddInvalidatingTemplateId(templateId);
            }
        }

        return cache;
    }

    public bool ClearCaches(Item item, ItemEventType eventType, bool remote)
    {
        var result = false;
        if (item != null)
        {
            foreach (var cache in _managedCaches.Values.ToArray())
            {
                result = cache.Clear(item, eventType, remote) || result;
            }
        }

        return result;
    }

    public List<string> GetCacheNames(bool includeUnmanaged = false)
    {
        if (includeUnmanaged)
        {
            return _cacheManagerHelper.GetAllCacheNames().OrderBy(s => s).ToList();
        }

        return _managedCaches.Keys.ToList();
    }

    public IManagedCache GetCacheByName(string cacheName, bool includeUnmanaged = false)
    {
        return _managedCaches.TryGetValue(cacheName, out var cache) ? cache :
            includeUnmanaged ? _cacheManagerHelper.GetCache(cacheName) : null;
    }

    public void ClearCache(string cacheName, bool includeUnmanaged = false)
    {
        GetCacheByName(cacheName, includeUnmanaged)?.Clear();
    }

    public bool IsManagedCache(IManagedCache cache)
    {
        return cache?.Name != null && _managedCaches.ContainsKey(cache.Name);
    }

    public void RaiseClearCacheEvent(string cacheName, string userName)
    {
        _queue.QueueEvent(
            new ManagedCacheInvalidationEvent(cacheName, userName, Settings.InstanceName),
            _raiseCacheClearEventsGlobally,
            _raiseCacheClearEventsLocally);
    }

    public void RaiseClearCacheEventOnRemotes(string cacheName, string userName)
    {
        _queue.QueueEvent(
            new ManagedCacheInvalidationEvent(cacheName, userName, Settings.InstanceName),
            true,
            false);
    }
}