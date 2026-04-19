using PerformantSitecore.Foundation.SxaSiteProvider.Helpers;

using Sitecore.Caching;
using Sitecore.XA.Foundation.Abstractions;
using Sitecore.XA.Foundation.Caching.Extensions;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Pipeline.SiteProviderInitialized
{
    public class SiteInfoResolverCacheClearer : BaseSiteProviderInitialized
    {
        private readonly ILoggingHelper _loggingHelper;
        private readonly IContext _context;

        public SiteInfoResolverCacheClearer(ILoggingHelper loggingHelper, IContext context)
        {
            _loggingHelper = loggingHelper;
            _context = context;
        }

        public override void OnProcess(SiteProviderInitializedPipelineArgs args)
        {
            var name = args.Item?.Database?.Name;

            if (string.IsNullOrWhiteSpace(name) ||
                _context.Items[Sitecore.XA.Foundation.Multisite.Constants.SiteInfoResolverCacheName] != null)
            {
                return;
            }

            _loggingHelper.LogDebug($"[SXA] SiteInfoResolverCacheClearer - clearing cache [{name}]", this);
            CacheManager.FindCacheByName<string>(Sitecore.XA.Foundation.Multisite.Constants.SiteInfoResolverCacheName)?.RemovePrefix(name);
            _context.Items[Sitecore.XA.Foundation.Multisite.Constants.SiteInfoResolverCacheName] = true;
            _loggingHelper.LogDebug("[SXA] SiteInfoResolverCacheClearer - done", this);
        }
    }
}
