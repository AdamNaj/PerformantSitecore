using PerformantSitecore.Foundation.SxaSiteProvider.Helpers;

using Microsoft.Extensions.DependencyInjection;

using Sitecore.DependencyInjection;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Configuration
{
    public class ServicesConfiguration : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILoggingHelper, LoggingHelper>();
            serviceCollection.AddSingleton<IPipelinesHelper, PipelinesHelper>();
            serviceCollection.AddSingleton<ISiteContextFactoryHelper, SiteContextFactoryHelper>();
            serviceCollection.AddSingleton<ISiteProviderHelper, SiteProviderHelper>();
        }
    }
}
