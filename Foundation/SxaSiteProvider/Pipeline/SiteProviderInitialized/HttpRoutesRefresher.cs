using System;

using PerformantSitecore.Foundation.SxaSiteProvider.Helpers;

using Sitecore.Events;
using Sitecore.Jobs.AsyncUI;
using Sitecore.XA.Foundation.SitecoreExtensions;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Pipeline.SiteProviderInitialized
{
    public class HttpRoutesRefresher : BaseSiteProviderInitialized
    {
        private readonly ILoggingHelper _logHelper;

        private readonly Sitecore.XA.Foundation.Multisite.EventHandlers.HttpRoutesRefresher _sxaRefresher =
            new Sitecore.XA.Foundation.Multisite.EventHandlers.HttpRoutesRefresher();

        public HttpRoutesRefresher(ILoggingHelper logHelper)
        {
            _logHelper = logHelper;
        }

        public override void OnProcess(SiteProviderInitializedPipelineArgs args)
        {
            if (JobContext.IsJob && !JobsHelper.IsPublishing())
            {
                try
                {
                    var refresherArgs = new SitecoreEventArgs("item:saved", new object[] { args.Item }, new EventResult());
                    _sxaRefresher.OnItemSaved(this, refresherArgs);
                }
                catch (Exception ex)
                {
                    _logHelper.LogError("Error while populating routes.", ex, this);
                }
            }
        }
    }
}
