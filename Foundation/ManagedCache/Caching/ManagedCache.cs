using System;
using System.Collections.Generic;
using System.Linq;

using PerformantSitecore.Foundation.ManagedCache.Services;

using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace PerformantSitecore.Foundation.ManagedCache.Caching;

public class ManagedCache : Sitecore.XA.Foundation.Caching.DictionaryCache, IManagedCache
{
    public List<ID> InvalidatingTemplateIds { get; }
    private readonly ManagedCacheInvalidationDelegate _cacheClearer;
    public DateTime LastCleared { get; private set; }
    public long Size => InnerCache.Size;
    public long MaxSize => InnerCache.MaxSize;
    public int Count => InnerCache.Count;
    public long RemainingSpace => InnerCache.RemainingSpace;

    public ManagedCache(string name, long maxSize,
        ManagedCacheInvalidationDelegate cacheClearer = null) : base(name, maxSize)
    {
        _cacheClearer = cacheClearer;
        InvalidatingTemplateIds = new List<ID>();
        LastCleared = DateTime.UtcNow;
    }

    public ManagedCache(string name, long maxSize, params ID[] invalidatingTemplateIds) : base(name, maxSize)
    {
        InvalidatingTemplateIds = invalidatingTemplateIds.ToList();
        LastCleared = DateTime.UtcNow;
    }

    public bool Clear(Item item, ItemEventType eventType, bool remote)
    {
        bool result = false;
        if (InvalidatingTemplateIds.Count > 0 &&
            InvalidatingTemplateIds.Exists(id => CheckItemInheritance(item, id)))
        {
            result = true;
            Clear();
        }

        if (_cacheClearer != null)
        {
            result = _cacheClearer(this, item, eventType, remote) || result;
        }

        return result;
    }

    public override void Clear()
    {
        Log.Info($"PerformantSitecore.ManagedCache: Clearing cache '{Name}'", this);
        base.Clear();
        LastCleared = DateTime.UtcNow;
    }

    public void AddInvalidatingTemplateId(ID id)
    {
        if (id != (ID)null && !InvalidatingTemplateIds.Contains(id))
        {
            InvalidatingTemplateIds.Add(id);
        }
    }

    public string[] GetCacheKeys()
    {
        return InnerCache.GetCacheKeys();
    }

    private static bool CheckItemInheritance(Item item, ID templateId)
    {
        if (item == null || templateId == (ID)null)
            return false;

        if (item.TemplateID == templateId)
            return true;

        var template = TemplateManager.GetTemplate(item);
        return template != null && template.InheritsFrom(templateId);
    }
}