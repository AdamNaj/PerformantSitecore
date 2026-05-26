using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace PerformantSitecore.Foundation.ManagedCache.Attributes;

/// <summary>
/// Sets Cache-Control response headers for Web API actions.
/// Defaults to no-cache / no-store which is appropriate for
/// the cache management endpoints.
/// </summary>
public class CacheControlApiHeaderAttribute(bool noCache = true, bool noStore = true, int maxAge = 0)
    : ActionFilterAttribute
{
    private bool NoStore { get; } = noStore;
    private bool NoCache { get; } = noCache;
    private int MaxAge { get; } = maxAge;

    public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
    {
        if (actionExecutedContext.Response == null)
            return;

        var cacheControl = actionExecutedContext.Response.Headers.CacheControl
                           ?? new CacheControlHeaderValue();

        cacheControl.NoCache = NoCache;
        cacheControl.NoStore = NoStore;
        cacheControl.MaxAge = MaxAge > 0
            ? TimeSpan.FromSeconds(MaxAge)
            : TimeSpan.Zero;

        actionExecutedContext.Response.Headers.CacheControl = cacheControl;
    }
}