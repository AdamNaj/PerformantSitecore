using System.Collections.Concurrent;

using Sitecore.Caching;
using Sitecore.Collections;
using Sitecore.Reflection;

namespace PerformantSitecore.Foundation.SqlDataProvider.Caching;

/// <summary>
/// A thread-safe dictionary that maps child item names to their ID lists.
/// Implements ICacheable so that Sitecore's cache infrastructure can
/// track its memory footprint accurately.
/// </summary>
public class ChildNamesCacheValue : ConcurrentDictionary<string, IdList>, ICacheable
{
    public long GetDataLength()
    {
        var size = TypeUtil.SizeOfDictionary();
        foreach (var kvp in this)
        {
            size += TypeUtil.SizeOfString(kvp.Key) +
                    TypeUtil.SizeOfList(kvp.Value) +
                    TypeUtil.SizeOfID() * kvp.Value.Count;
        }

        return size;
    }

    public bool Cacheable { get; set; } = true;
    public bool Immutable => false;
#pragma warning disable 67
    public event DataLengthChangedDelegate DataLengthChanged;
#pragma warning restore 67
}