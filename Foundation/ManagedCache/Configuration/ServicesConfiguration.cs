using Microsoft.Extensions.DependencyInjection;

using PerformantSitecore.Foundation.ManagedCache.Controllers;
using PerformantSitecore.Foundation.ManagedCache.Helpers;
using PerformantSitecore.Foundation.ManagedCache.Services;

using Sitecore.DependencyInjection;

namespace PerformantSitecore.Foundation.ManagedCache.Configuration;

public class ServicesConfiguration : IServicesConfigurator
{
    public void Configure(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ICacheManagerHelper, CacheManagerHelper>();
        serviceCollection.AddSingleton<IManagedCacheService, ManagedCacheService>();
        serviceCollection.AddTransient<CacheInsightsController>();
    }
}