using Sitecore.Sites;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Helpers
{
    public interface ISiteProviderHelper
    {
        SiteProviderCollection GetAllSiteProviders();
        bool IsAsyncSxaSiteProviderReloading { get; }
    }
}
