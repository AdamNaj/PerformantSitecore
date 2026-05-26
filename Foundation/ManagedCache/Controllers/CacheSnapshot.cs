using System;
using System.Runtime.Serialization;

using PerformantSitecore.Foundation.ManagedCache.Extensions;

using Sitecore.Data;

namespace PerformantSitecore.Foundation.ManagedCache.Controllers;

[DataContract]
public class CacheSnapshot
{
    [DataMember(Name = "Managed Cache")]
    public bool IsManagedCache { get; set; }

    [DataMember(Name = "Currently allocated cache space")]
    public string ReadableCurrentSize => Size.ToSizeString(1);

    [DataMember(Name = "Remaining allowed space")]
    public string ReadableRemainingSpace => RemainingSpace.ToSizeString(1);

    [DataMember(Name = "Max allowed cache space")]
    public string ReadableMaxSize => MaxSize.ToSizeString(1);

    [DataMember]
    public long Size { get; set; }

    [DataMember]
    public long MaxSize { get; set; }

    [DataMember]
    public long RemainingSpace { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public DateTime LastCleared { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public ID[] InvalidatingTemplateIds { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public int Count { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public object Keys { get; set; }
}