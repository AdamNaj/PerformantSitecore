using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

using PerformantSitecore.Foundation.ManagedCache.Attributes;
using PerformantSitecore.Foundation.ManagedCache.Caching;
using PerformantSitecore.Foundation.ManagedCache.Extensions;
using PerformantSitecore.Foundation.ManagedCache.Services;

using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Services.Infrastructure.Web.Http.Filters;

namespace PerformantSitecore.Foundation.ManagedCache.Controllers;

[AnonymousUserFilter(AllowAnonymous = AllowAnonymousOptions.Allow)]
[RoutePrefix("api/v1/managedcache")]
public class CacheInsightsController(IManagedCacheService managedCacheService) : ApiController
{
    public enum SortBy
    {
        Name,
        Size,
        Count,
        Unsorted
    }

    private readonly int _maxInspectedKeys = Settings.GetIntSetting(
        "PerformantSitecore.CacheInsights.MaxInspectedKeys", 10);

    private CacheOperationResult Wrap(object data, string name,
        string message = "success", bool success = true)
    {
        return new CacheOperationResult
        {
            Url = Request.RequestUri,
            Data = data,
            Instance = Dns.GetHostName(),
            CacheName = name,
            Message = message,
            Success = success,
            Timestamp = DateTime.Now
        };
    }

    private CacheOperationResult ExecuteForCache(
        string cacheNamePattern, bool includeUnmanaged, SortBy sortBy,
        Func<IManagedCache, CacheSnapshot> func)
    {
        var matchingCacheNames = managedCacheService.GetCacheNames(includeUnmanaged);
        if (!string.IsNullOrWhiteSpace(cacheNamePattern))
        {
            matchingCacheNames = cacheNamePattern
                .WildcardFilter(matchingCacheNames, name => name).ToList();
        }

        if (!matchingCacheNames.Any())
        {
            return Wrap(null, cacheNamePattern, "Cache not found", false);
        }

        var result = new Dictionary<string, CacheSnapshot>();
        long size = 0;
        int cacheCount = 0;
        foreach (var cacheName in matchingCacheNames)
        {
            var cache = managedCacheService.GetCacheByName(cacheName, includeUnmanaged);
            if (cache == null)
                continue;
            var cacheResult = func(cache);
            if (cacheResult != null)
            {
                size += cache.Size;
                cacheCount++;
                result.Add(cacheName, cacheResult);
            }
        }

        switch (sortBy)
        {
            case SortBy.Name:
                result = result.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                break;
            case SortBy.Size:
                result = result.OrderByDescending(x => x.Value.Size).ToDictionary(x => x.Key, x => x.Value);
                break;
            case SortBy.Count:
                result = result.OrderByDescending(x => x.Value.Count).ToDictionary(x => x.Key, x => x.Value);
                break;
            case SortBy.Unsorted:
            default:
                break;
        }

        var apiResponse = Wrap(result, cacheNamePattern);
        apiResponse.MatchingCacheCount = cacheCount;
        apiResponse.MatchingCacheSize = size;
        return apiResponse;
    }

    [HttpGet]
    [Route("flush")]
    [CacheControlApiHeader]
    [RequireApiKey]
    [TargetInstance]
    public IHttpActionResult Flush(
        [FromUri] string cacheName, [FromUri] string keys = "",
        [FromUri] bool includeUnmanaged = false, [FromUri] SortBy sortBy = SortBy.Name,
        [FromUri] bool propagate = false)
    {
        // propagate fans out a *full* cache clear to every other instance via
        // the event queue. The remote handler can only clear whole caches, so
        // we refuse to silently over-clear remotes when a key filter is set.
        var fullClear = string.IsNullOrWhiteSpace(keys) || keys == "*";
        var canPropagate = propagate && fullClear;
        var userName = Sitecore.Context.User?.Name ?? "managedcache-api";
        var propagatedCount = 0;

        var response = ExecuteForCache(cacheName, includeUnmanaged, sortBy, cache =>
        {
            if (fullClear)
            {
                cache.Clear();
                if (canPropagate)
                {
                    managedCacheService.RaiseClearCacheEventOnRemotes(cache.Name, userName);
                    propagatedCount++;
                }
                return GetCacheSnapshot(cache, string.Empty);
            }

            bool keyFound = false;
            keys.WildcardFilter(cache.GetCacheKeys()).ForEach(key =>
            {
                cache.Remove(key);
                keyFound = true;
            });

            return keyFound ? GetCacheSnapshot(cache, string.Empty) : null;
        });

        if (response.Success)
        {
            if (propagate && !fullClear)
                response.Message = "success (propagate ignored: remote clear is whole-cache only; pass keys=* to fan out)";
            else if (propagatedCount > 0)
                response.Message = $"success (clear event queued for {propagatedCount} cache(s) on remote instances)";
        }

        return Json(response);
    }

    [HttpGet]
    [Route("inspect")]
    [CacheControlApiHeader]
    [RequireApiKey]
    [TargetInstance]
    public IHttpActionResult Inspect(
        [FromUri] string cacheName, [FromUri] string keys = "",
        [FromUri] bool includeUnmanaged = false, [FromUri] SortBy sortBy = SortBy.Name)
    {
        return Json(ExecuteForCache(cacheName, includeUnmanaged, sortBy, cache =>
        {
            var maxKeys = _maxInspectedKeys;
            var data = new SortedDictionary<string, object>();
            keys.WildcardFilter(cache.GetCacheKeys()).ForEach(key =>
            {
                if (maxKeys > 0)
                {
                    var entry = cache.Get(key);
                    data.Add(key, entry?.Value);
                    maxKeys--;
                }
            });

            var result = GetCacheSnapshot(cache, "");
            result.Keys = data;

            return maxKeys != _maxInspectedKeys ? result : null;
        }));
    }

    [HttpGet]
    [Route("statistics")]
    [CacheControlApiHeader]
    [RequireApiKey]
    [TargetInstance]
    public IHttpActionResult GetStatistics(
        [FromUri] string cacheName, [FromUri] string keys = "",
        [FromUri] bool includeUnmanaged = false, [FromUri] SortBy sortBy = SortBy.Name)
    {
        return Json(ExecuteForCache(cacheName, includeUnmanaged, sortBy,
            cache => GetCacheSnapshot(cache, keys)));
    }

    [HttpGet]
    [Route("list")]
    [CacheControlApiHeader]
    [RequireApiKey]
    [TargetInstance]
    public IHttpActionResult List(
        [FromUri] string cacheName, [FromUri] string keys = "",
        [FromUri] bool includeUnmanaged = false, [FromUri] SortBy sortBy = SortBy.Name)
    {
        return Json(ToCacheListEntry(ExecuteForCache(cacheName, includeUnmanaged, sortBy,
            cache => GetCacheSnapshot(cache, keys))));
    }

    private CacheOperationResult ToCacheListEntry(CacheOperationResult response)
    {
        if (response.Data is Dictionary<string, CacheSnapshot> cacheStatistics)
            response.Data = cacheStatistics.ToDictionary(x => x.Key, x => x.Value.ReadableCurrentSize);
        return response;
    }

    private CacheSnapshot GetCacheSnapshot(IManagedCache cache, string keys)
    {
        var result = new CacheSnapshot
        {
            Size = cache.Size,
            MaxSize = cache.MaxSize,
            RemainingSpace = cache.RemainingSpace,
            LastCleared = cache.LastCleared,
            InvalidatingTemplateIds = cache.InvalidatingTemplateIds?.ToArray(),
            Count = cache.Count,
            IsManagedCache = managedCacheService.IsManagedCache(cache),
            Keys = null
        };

        string[] filteredKeys;
        switch (keys)
        {
            case null:
            case "":
                return result;
            case "*":
                filteredKeys = cache.GetCacheKeys();
                break;
            default:
                filteredKeys = keys.WildcardFilter(cache.GetCacheKeys()).ToArray();
                break;
        }

        if (filteredKeys.Any())
        {
            result.Keys = filteredKeys;
            return result;
        }

        return null;
    }
}