using PerformantSitecore.Foundation.SxaSiteProvider.Helpers;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Pipeline.SiteProviderInitialized
{
    public class SiteContextFactoryReset : BaseSiteProviderInitialized
    {
        private readonly ISiteContextFactoryHelper _siteContextFactoryHelper;

        public SiteContextFactoryReset(ISiteContextFactoryHelper siteContextFactoryHelper)
        {
            _siteContextFactoryHelper = siteContextFactoryHelper;
        }

        public override void OnProcess(SiteProviderInitializedPipelineArgs args)
        {
            _siteContextFactoryHelper.Reset();
        }
    }
}
