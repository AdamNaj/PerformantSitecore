using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using PerformantSitecore.Foundation.ManagedCache.Services;

using Sitecore.Caching;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.XA.Foundation.Caching;

namespace PerformantSitecore.Foundation.ManagedCache.Caching;

[ExcludeFromCodeCoverage]
public class UnmanagedCacheAdapter : IManagedCache
{
    private readonly ICacheInfo _inner;
    private readonly Cache _cache;

    public UnmanagedCacheAdapter(ICacheInfo inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = inner as Cache;
    }

    public string Name => _inner.Name;
    public long Size => _inner.Size;
    public long MaxSize => _inner.MaxSize;
    public int Count => _inner.Count;
    public long RemainingSpace => _inner.RemainingSpace;
    public bool Enabled => _inner.Enabled;
    public DateTime LastCleared => DateTime.MinValue;
    public List<ID> InvalidatingTemplateIds => null;

    public string[] GetCacheKeys()
    {
        return _cache?.GetCacheKeys() ?? Array.Empty<string>();
    }

    public void Clear()
    {
        _inner.Clear();
    }

    public void Remove(string key)
    {
        _cache?.Remove(key);
    }

    public bool Clear(Item item, ItemEventType eventType, bool remote) => false;
    public void Set(ID id, DictionaryCacheValue value) { }
    public void Set(string id, DictionaryCacheValue value) { }
    public DictionaryCacheValue Get(ID id) => null;
    public DictionaryCacheValue Get(string id) => null;
    public void AddInvalidatingTemplateId(ID id) { }
}
