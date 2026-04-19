using Sitecore.XA.Foundation.Multisite;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Pipeline.SiteProviderInitialized
{
    public class SiteInfoResolverReset : BaseSiteProviderInitialized
    {
        private readonly ISiteInfoResolver _siteInfoResolver;

        public SiteInfoResolverReset(ISiteInfoResolver siteInfoResolver)
        {
            _siteInfoResolver = siteInfoResolver;
        }

        public override void OnProcess(SiteProviderInitializedPipelineArgs args)
        {
            _siteInfoResolver.Reset();
        }
    }
}
