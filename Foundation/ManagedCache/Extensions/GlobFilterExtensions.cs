using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PerformantSitecore.Foundation.ManagedCache.Extensions;

public static class GlobFilterExtensions
{
    public static WildcardPattern GetWildcardPattern(this string name)
    {
        if (string.IsNullOrEmpty(name))
            name = "*";
        return new WildcardPattern(name, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
    }

    public static IEnumerable<T> WildcardFilter<T>(
        this string filter,
        IEnumerable<T> items,
        Func<T, string> propertyName)
    {
        var wildcardPattern = filter.GetWildcardPattern();
        return items.Where(item => wildcardPattern.IsMatch(propertyName(item)));
    }

    public static IEnumerable<string> WildcardFilter(
        this string filter,
        IEnumerable<string> items)
    {
        var wildcardPattern = filter.GetWildcardPattern();
        return items.Where(item => wildcardPattern.IsMatch(item));
    }
}