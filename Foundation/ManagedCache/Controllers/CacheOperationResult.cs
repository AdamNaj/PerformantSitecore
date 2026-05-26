using System;
using System.Runtime.Serialization;

using PerformantSitecore.Foundation.ManagedCache.Extensions;

namespace PerformantSitecore.Foundation.ManagedCache.Controllers;

[DataContract]
public class CacheOperationResult
{
    [DataMember]
    public Uri Url { get; set; }

    [DataMember]
    public string Instance { get; set; }

    [DataMember]
    public string CacheName { get; set; }

    [DataMember]
    public string Message { get; set; }

    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public DateTime Timestamp { get; set; }

    [DataMember]
    public long MatchingCacheCount { get; set; }

    [DataMember]
    public long MatchingCacheSize { get; set; }

    [DataMember(Name = "Memory consumed by listed caches")]
    public string ReadableMatchingCacheSize => MatchingCacheSize.ToSizeString(1);

    [DataMember]
    public object Data { get; set; }
}