using System.Linq;

using PerformantSitecore.Foundation.SxaSiteProvider.Helpers;

using Sitecore.Abstractions;
using Sitecore.Caching;
using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Jobs.AsyncUI;
using Sitecore.Sites;
using Sitecore.XA.Foundation.Multisite.Extensions;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Pipeline.SiteProviderInitialized
{
    public class AllSxaSiteCacheClearer : BaseSiteProviderInitialized
    {
        private readonly ILoggingHelper _logHelper;
        private readonly BaseSiteManager _siteManager;

        public AllSxaSiteCacheClearer(ILoggingHelper logHelper, BaseSiteManager siteManager)
        {
            _logHelper = logHelper;
            _siteManager = siteManager;
        }

        public override void OnProcess(SiteProviderInitializedPipelineArgs args)
        {
            if (JobContext.IsJob)
            {
                _siteManager.GetSites().Where(site => site.IsSxaSite()).Select(site => site.Name)
                    .ForEach(ClearSiteCache);
            }
        }

        private void ClearSiteCache(string siteName)
        {
            _logHelper.LogInfo($"HtmlCacheClearer clearing cache for {siteName} site", this);
            ProcessSite(siteName);
            _logHelper.LogInfo("HtmlCacheClearer done.", this);
        }

        private void ProcessSite(string siteName)
        {
            SiteContext site = Factory.GetSite(siteName);
            if (site != null)
            {
                HtmlCache htmlCache = CacheManager.GetHtmlCache(site);
                htmlCache?.Clear();
            }
        }
    }
}
