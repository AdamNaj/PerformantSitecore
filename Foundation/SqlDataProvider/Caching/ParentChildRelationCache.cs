using Sitecore.Caching;
using Sitecore.Data;

namespace PerformantSitecore.Foundation.SqlDataProvider.Caching;

/// <summary>
/// A per-database cache that maps parent item IDs to their
/// child-name lookup results. Built on Sitecore's native Cache
/// so it participates in the standard /sitecore/admin/cache.aspx page.
/// </summary>
public class ParentChildRelationCache(string name, long maxSize) : CustomCache(name, maxSize)
{
    public ChildNamesCacheValue GetRelations(ID parentId)
    {
        var key = parentId.ToString();
        var result = GetObject(key) as ChildNamesCacheValue;
        if (result == null)
        {
            result = new ChildNamesCacheValue();
            SetObject(key, result);
        }

        return result;
    }
}