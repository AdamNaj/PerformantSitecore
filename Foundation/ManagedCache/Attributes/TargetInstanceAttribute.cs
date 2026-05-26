using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using PerformantSitecore.Foundation.ManagedCache.Extensions;

using Sitecore.Configuration;
using Sitecore.StringExtensions;

namespace PerformantSitecore.Foundation.ManagedCache.Attributes;

/// <summary>
/// Ensures the "instanceName" query parameter matches the current
/// server's hostname. Supports wildcard patterns when AllowWildcards
/// is true (the default). Use instanceName=* to target whichever
/// instance the load balancer routes to.
/// </summary>
public class TargetInstanceAttribute(bool validateInstance = true, bool allowWildcards = true) : ActionFilterAttribute
{
    private bool ValidateInstance { get; } = validateInstance;
    private bool AllowWildcards { get; } = allowWildcards;

    public override void OnActionExecuting(HttpActionContext actionContext)
    {
        var controllerName =
            actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;

        var validate = Settings.GetBoolSetting(
            $"{controllerName}.ValidateInstanceName", ValidateInstance);
        var allowWildcards = Settings.GetBoolSetting(
            $"{controllerName}.AllowWildcards", AllowWildcards);

        if (!validate)
            return;

        var instanceName = Dns.GetHostName();
        var query = actionContext.Request.RequestUri.ParseQueryString();

        if (!query.AllKeys.Contains("instanceName", StringComparer.OrdinalIgnoreCase))
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "Instance name is missing"
            };
            return;
        }

        var requestInstanceName = query["instanceName"]?.Trim();

        if (!allowWildcards)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(requestInstanceName, instanceName))
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Instance name mismatch"
                };
            }
        }
        else if (requestInstanceName.IsNullOrEmpty() ||
                 !requestInstanceName.GetWildcardPattern().IsMatch(instanceName))
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "Instance name mismatch"
            };
        }
    }
}