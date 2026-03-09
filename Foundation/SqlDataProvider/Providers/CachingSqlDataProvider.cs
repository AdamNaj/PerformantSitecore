using PerformantSitecore.Foundation.SqlDataProvider.Caching;

using Sitecore;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;

namespace PerformantSitecore.Foundation.SqlDataProvider.Providers;

/// <summary>
/// A thin caching layer over Sitecore's SqlServerDataProvider that
/// intercepts GetChildIdsByName calls and caches the results.
///
/// Sitecore calls GetChildIdsByName hundreds of times per request
/// to resolve child items by name under a given parent. Each call
/// hits SQL Server. This provider caches those lookups in memory,
/// reducing SQL round-trips by a factor of 7-10x on a typical site.
///
/// Wire it in via Sitecore config patching - see the accompanying
/// PerformantSitecore.Foundation.SqlDataProvider.config.
/// </summary>
public class CachingSqlDataProvider(string connectionString)
    : Sitecore.Data.SqlServer.SqlServerDataProvider(connectionString)
{
    private static DatabaseCacheMap _cacheMap;
    private ParentChildRelationCache _cache;

    public static bool CacheEnabled { get; set; } = true;

    public static DatabaseCacheMap CacheMap
    {
        get
        {
            if (_cacheMap == null)
            {
                string cacheName = Settings.GetSetting(
                    "PerformantSitecore.SqlDataProvider.CacheName",
                    "PerformantSitecore.ChildParentRelation");

                long cacheMaxSize = StringUtil.ParseSizeString(
                    Settings.GetSetting(
                        "PerformantSitecore.SqlDataProvider.CacheMaxSize", "10MB"));

                _cacheMap = new DatabaseCacheMap(cacheName, cacheMaxSize);

                CacheEnabled = Settings.GetBoolSetting(
                    "PerformantSitecore.SqlDataProvider.CacheEnabled", true);
            }

            return _cacheMap;
        }
    }

    public bool IsCaching
    {
        get
        {
            if (_cache == null)
            {
                _cache = CacheMap.GetCacheForDatabase(Name);
                _cache.Enabled = Settings.GetBoolSetting(
                    $"PerformantSitecore.SqlDataProvider.{Name}.CacheEnabled", true);
            }

            return _cache.Enabled;
        }
        set
        {
            CacheMap.GetCacheForDatabase(Name).Enabled = value;
        }
    }

    private ParentChildRelationCache Cache
    {
        get
        {
            if (_cache == null)
            {
                _cache = CacheMap.GetCacheForDatabase(Name);
            }

            return _cache;
        }
    }

    protected override IdList GetChildIdsByName(string childName, ID parentId)
    {
        if (!CacheEnabled || !IsCaching)
        {
            return base.GetChildIdsByName(childName, parentId);
        }

        var result = new IdList();
        var item = Database.GetItem(parentId);
        if (item == null)
        {
            return result;
        }

        var children = Cache.GetRelations(item.ID);
        result = children.GetOrAdd(childName, _ => base.GetChildIdsByName(childName, parentId));
        return result;
    }

    public static void ClearCache(string databaseName)
    {
        if (CacheEnabled)
        {
            var cache = CacheMap.GetCacheForDatabase(databaseName);
            if (cache.Enabled)
            {
                cache.Clear();
            }
        }
    }
}