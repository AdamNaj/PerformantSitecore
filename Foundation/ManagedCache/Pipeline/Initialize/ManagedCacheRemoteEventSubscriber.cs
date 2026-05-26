using System;

using PerformantSitecore.Foundation.ManagedCache.EventHandlers;

using Sitecore.Data.Events;
using Sitecore.Eventing;
using Sitecore.Events;
using Sitecore.Pipelines;

namespace PerformantSitecore.Foundation.ManagedCache.Pipeline.Initialize;

/// <summary>
/// Bridges <see cref="ManagedCacheInvalidationEvent"/> instances arriving
/// on the Sitecore event queue to the regular Sitecore event pipeline:
/// when the event is received on a remote instance, it is re-raised as a
/// named Sitecore event so handlers wired up in &lt;events&gt; config
/// actually fire. Without this initializer the cross-instance cache
/// invalidation handlers are orphaned.
/// </summary>
public class ManagedCacheRemoteEventSubscriber
{
    public void Initialize(PipelineArgs args)
    {
        EventManager.Subscribe(new Action<ManagedCacheInvalidationEvent>(OnRemoteEvent));
    }

    private static void OnRemoteEvent<TEvent>(TEvent @event) where TEvent : IHasEventName
    {
        var remoteEventArgs = new RemoteEventArgs<TEvent>(@event);
        Event.RaiseEvent(@event.EventName, remoteEventArgs);
    }
}
