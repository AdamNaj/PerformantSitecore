using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Sitecore.Configuration;

namespace PerformantSitecore.Foundation.ManagedCache.Attributes;

/// <summary>
/// Requires an API key provided as a query string parameter, validated
/// against keys configured in Sitecore settings. The setting name is
/// derived from the controller and action name, e.g.
/// "CacheInsights.Flush.KeyValues" for the Flush action.
/// Multiple pipe-separated keys are supported.
/// </summary>
public class RequireApiKeyAttribute(string keyParameter = "apiKey") : ActionFilterAttribute
{
    public string KeyParameter { get; set; } = keyParameter;

    public override void OnActionExecuting(HttpActionContext actionContext)
    {
        var serviceSignature =
            actionContext.ActionDescriptor.ControllerDescriptor.ControllerName + "." +
            actionContext.ActionDescriptor.ActionName;

        var keyParameter = Settings.GetSetting($"{serviceSignature}.KeyParameter", KeyParameter);
        var keyValues = Settings.GetSetting($"{serviceSignature}.KeyValues", "").Split('|');

        var query = actionContext.Request.RequestUri.ParseQueryString();
        if (!query.AllKeys.Contains(keyParameter, StringComparer.OrdinalIgnoreCase))
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "API key is missing"
            };
            return;
        }

        var requestApiKeyValue = query[keyParameter]?.Trim();
        if (string.IsNullOrWhiteSpace(requestApiKeyValue))
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "API key not provided"
            };
        }
        else if (!keyValues.Any(key => string.Equals(key, requestApiKeyValue)))
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "API key not valid"
            };
        }
    }
}