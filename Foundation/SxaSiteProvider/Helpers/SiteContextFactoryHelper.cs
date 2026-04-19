using Sitecore.Sites;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Helpers
{
    public class SiteContextFactoryHelper : ISiteContextFactoryHelper
    {
        public void Reset()
        {
            SiteContextFactory.Reset();
        }
    }
}
