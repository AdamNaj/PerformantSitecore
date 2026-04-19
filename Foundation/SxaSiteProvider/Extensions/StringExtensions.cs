using Sitecore;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Extensions
{
    public static class StringExtensions
    {
        public static string EnsurePrefix(this string str, char prefix)
        {
            return StringUtil.EnsurePrefix(prefix, str);
        }
    }
}
