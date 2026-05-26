using System;
using System.Collections.Generic;

using PerformantSitecore.Foundation.ManagedCache.Services;

using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.XA.Foundation.Caching;

namespace PerformantSitecore.Foundation.ManagedCache.Caching;

public interface IManagedCache
{
    List<ID> InvalidatingTemplateIds { get; }
    void Set(ID id, DictionaryCacheValue value);
    void Set(string id, DictionaryCacheValue value);
    void Remove(string key);
    DictionaryCacheValue Get(ID id);
    DictionaryCacheValue Get(string id);
    bool Clear(Item item, ItemEventType eventType, bool remote);
    void Clear();
    void AddInvalidatingTemplateId(ID id);
    string[] GetCacheKeys();
    string Name { get; }
    long Size { get; }
    long MaxSize { get; }
    int Count { get; }
    long RemainingSpace { get; }
    bool Enabled { get; }
    DateTime LastCleared { get; }
}