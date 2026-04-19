using PerformantSitecore.Foundation.SxaSiteProvider.Helpers;

using Sitecore.Caching;
using Sitecore.XA.Foundation.Caching.Extensions;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Pipeline.SiteProviderInitialized
{
    public class MultisiteContextCacheClearer : BaseSiteProviderInitialized
    {
        private readonly ILoggingHelper _loggingHelper;

        public MultisiteContextCacheClearer(ILoggingHelper loggingHelper)
        {
            _loggingHelper = loggingHelper;
        }

        public override void OnProcess(SiteProviderInitializedPipelineArgs args)
        {
            var name = args?.Item?.Database?.Name;
            if (!string.IsNullOrWhiteSpace(name))
            {
                _loggingHelper.LogDebug($"MultisiteContextCacheClearer - clearing cache [{name}]", this);
                CacheManager
                    .FindCacheByName<string>(Sitecore.XA.Foundation.Multisite.Constants.MultisiteContextCacheName)
                    ?.RemovePrefix(name);
                _loggingHelper.LogDebug("MultisiteContextCacheClearer - done", this);
            }
        }
    }
}
