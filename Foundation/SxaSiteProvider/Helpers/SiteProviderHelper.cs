using System.Linq;

using PerformantSitecore.Foundation.SxaSiteProvider.Providers;

using Microsoft.Extensions.DependencyInjection;

using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.DependencyInjection;
using Sitecore.Sites;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Helpers
{
    public class SiteProviderHelper : ISiteProviderHelper
    {
        public SiteProviderCollection GetAllSiteProviders()
        {
            var providerHelper = ServiceLocator.ServiceProvider
                .GetService<ProviderHelper<SiteProvider, SiteProviderCollection>>();
            return providerHelper.Providers;
        }

        public bool IsAsyncSxaSiteProviderReloading
        {
            get
            {
                var provider = GetAllSiteProviders().OfType<IAsyncSxaSiteProvider>()
                    .FirstOrDefault();
                return provider != null && provider.IsReloading;
            }
        }
    }
}
