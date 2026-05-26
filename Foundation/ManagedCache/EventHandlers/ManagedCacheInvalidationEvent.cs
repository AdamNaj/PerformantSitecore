using System.Runtime.Serialization;

using Sitecore.Eventing;

namespace PerformantSitecore.Foundation.ManagedCache.EventHandlers;

[DataContract]
public class ManagedCacheInvalidationEvent(string cacheName, string userName, string instanceName) : IHasEventName
{
    public string EventName => "performantsitecore:managedcache:invalidation";

    [DataMember]
    public string InstanceName { get; protected set; } = instanceName;

    [DataMember]
    public string CacheName { get; set; } = cacheName;

    [DataMember]
    public string UserName { get; set; } = userName;
}