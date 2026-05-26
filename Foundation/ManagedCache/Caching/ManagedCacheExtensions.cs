using System;
using System.Collections.Generic;
using System.Linq;

using Sitecore.StringExtensions;
using Sitecore.XA.Foundation.Caching;

namespace PerformantSitecore.Foundation.ManagedCache.Caching;

public static class ManagedCacheExtensions
{
    /// <summary>
    /// Retrieves an object from cache or calls the factory method to produce it,
    /// then stores the result in cache. If the cache reference is null, the factory
    /// method is called directly - making this extension transparent to whether
    /// caching is enabled. This is the key method that makes the cache easy to
    /// embed and easy to unit test (just pass null for the cache).
    /// </summary>
    public static T GetOrSetIfNotCached<T>(this IManagedCache cache, string cacheKey,
        Func<T> getObject, Func<T, string> objectToString, Func<string, T> stringToObject)
        where T : class
    {
        if (cache != null)
        {
            var cacheValue = cache.Get(cacheKey);

            if (cacheValue != null)
            {
                var sCachedObject = cacheValue.Value;
                if (sCachedObject.IsNullOrEmpty())
                {
                    return null;
                }

                return stringToObject(sCachedObject);
            }
        }

        var value = getObject();

        if (cache != null && value != null)
        {
            string cachedValue = objectToString(value);
            cache.Set(cacheKey, new DictionaryCacheValue { Value = cachedValue });
        }

        return value;
    }

    public static void Set<T>(this IManagedCache cache, string cacheKey, T value,
        Func<T, string> objectToString) where T : class
    {
        if (cache != null && value != null)
        {
            string cachedValue = objectToString(value);
            cache.Set(cacheKey, new DictionaryCacheValue { Value = cachedValue });
        }
    }

    public static IDictionary<string, T> Get<T>(this IManagedCache cache,
        IEnumerable<string> cacheKeys, Func<string, T> stringToObject) where T : class
    {
        if (cache == null || cacheKeys == null)
        {
            return null;
        }

        var cacheKeysArray = cacheKeys.ToArray();
        var result = new Dictionary<string, T>();
        foreach (var cacheKey in cacheKeysArray)
        {
            var cacheValue = cache.Get(cacheKey);

            if (cacheValue == null || cacheValue.Value.IsNullOrEmpty())
            {
                continue;
            }

            result.Add(cacheKey, stringToObject(cacheValue.Value));
        }

        return result;
    }

    public static T Get<T>(this IManagedCache cache, string cacheKey,
        Func<string, T> stringToObject) where T : class
    {
        if (cache == null || cacheKey.IsNullOrEmpty())
        {
            return null;
        }

        var cacheValue = cache.Get(cacheKey);
        var result = stringToObject(cacheValue.Value);
        return result;
    }
}