using System;

using PerformantSitecore.Foundation.ManagedCache.Services;

using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Publishing;

namespace PerformantSitecore.Foundation.ManagedCache.EventHandlers;

public class ManagedCacheInvalidationHandler(IManagedCacheService managedCacheService)
{
    protected virtual void ClearCache(Item item, ItemEventType eventType, bool remote)
    {
        if (item != null && managedCacheService.ClearCaches(item, eventType, remote))
        {
            Log.Info(
                $"PerformantSitecore.ManagedCache: Cache cleared for item '{item.Name}' {item.ID} " +
                $"in DB:{item.Database?.Name}, of template '{item.TemplateName}' " +
                $"after {(remote ? "remote" : "local")} {eventType} event",
                this);
        }
    }

    protected virtual void ClearCacheRequest(string cacheName, string userName, bool remote)
    {
        managedCacheService.ClearCache(cacheName);
        Log.Info(
            $"PerformantSitecore.ManagedCache: {(remote ? "Remote" : "Local")} cache clear request " +
            $"by '{userName}' received for cache '{cacheName}'",
            this);
    }

    protected virtual void OnItemCopied(object sender, Item item, bool remote)
    {
        ClearCache(item, ItemEventType.CopyItem, remote);
    }

    protected virtual void OnItemCreated(object sender, Item item, bool remote)
    {
        ClearCache(item, ItemEventType.CreateItem, remote);
    }

    protected virtual void OnItemDeleted(object sender, Item item, bool remote)
    {
        ClearCache(item, ItemEventType.DeleteItem, remote);
    }

    protected virtual void OnItemMoved(object sender, Item item, bool remote)
    {
        ClearCache(item, ItemEventType.MoveItem, remote);
    }

    protected virtual void OnPublish(object sender, Item item, bool remote)
    {
        ClearCache(item, ItemEventType.PublishItem, remote);
    }

    protected virtual void OnItemRenamed(object sender, Item item, bool remote)
    {
        ClearCache(item, ItemEventType.RenameItem, remote);
    }

    protected virtual void OnItemSaved(object sender, Item item, bool remote)
    {
        ClearCache(item, ItemEventType.SaveItem, remote);
    }

    public void OnCacheCleared(object sender, EventArgs args)
    {
        Assert.ArgumentNotNull(sender, nameof(sender));
        Assert.ArgumentNotNull(args, nameof(args));

        if (!(args is RemoteEventArgs<ManagedCacheInvalidationEvent> cacheClearEventArgs))
            return;
        ClearCacheRequest(
            cacheClearEventArgs.Event.CacheName,
            cacheClearEventArgs.Event.UserName,
            false);
    }

    public void OnCacheClearedRemote(object sender, EventArgs args)
    {
        Assert.ArgumentNotNull(sender, nameof(sender));
        Assert.ArgumentNotNull(args, nameof(args));
        if (!(args is RemoteEventArgs<ManagedCacheInvalidationEvent> cacheClearEventArgs))
            return;
        ClearCacheRequest(
            cacheClearEventArgs.Event.CacheName,
            cacheClearEventArgs.Event.UserName,
            true);
    }

    public void OnItemCreated(object sender, EventArgs args)
    {
        var arg = Event.ExtractParameter(args, 0) as ItemCreatedEventArgs;
        OnItemCreated(sender, arg?.Item, false);
    }

    public void OnPublishEnd(object sender, EventArgs args)
    {
        if (Event.ExtractParameter(args, 0) is Publisher parameter &&
            parameter.Options.Mode == PublishMode.SingleItem && !parameter.Options.Deep)
        {
            OnPublish(sender, parameter.Options.RootItem, false);
        }
        else
        {
            var item = Event.ExtractParameter(args, 0) as Item;
            OnPublish(sender, item, false);
        }
    }

    public void OnPublishEndRemote(object sender, EventArgs args)
    {
        Assert.ArgumentNotNull(sender, nameof(sender));
        Assert.ArgumentNotNull(args, nameof(args));
        if (!(args is PublishEndRemoteEventArgs endRemoteEventArgs))
            return;
        var db = Sitecore.Configuration.Factory.GetDatabase(endRemoteEventArgs.TargetDatabaseName);
        var item = db?.GetItem(ID.Parse(endRemoteEventArgs.RootItemId));
        OnPublish(sender, item, true);
    }

    public void OnItemMoved(object sender, EventArgs args)
    {
        Assert.ArgumentNotNull(sender, nameof(sender));
        Assert.ArgumentNotNull(args, nameof(args));
        var item = Event.ExtractParameter(args, 0) as Item;
        OnItemMoved(sender, item, false);
    }

    public void OnItemMovedRemote(object sender, EventArgs args)
    {
        Assert.ArgumentNotNull(sender, nameof(sender));
        Assert.ArgumentNotNull(args, nameof(args));
        if (!(args is ItemMovedRemoteEventArgs movedRemoteEventArgs))
            return;
        OnItemMoved(sender, movedRemoteEventArgs.Item, true);
    }

    public void OnItemRenamed(object sender, EventArgs args)
    {
        Assert.ArgumentNotNull(sender, nameof(sender));
        Assert.ArgumentNotNull(args, nameof(args));
        var item = Event.ExtractParameter(args, 0) as Item;
        OnItemRenamed(sender, item, false);
    }

    public void OnItemRenamedRemote(object sender, EventArgs args)
    {
        Assert.ArgumentNotNull(sender, nameof(sender));
        Assert.ArgumentNotNull(args, nameof(args));
        var savedRemoteEventArgs = args as ItemSavedRemoteEventArgs;
        var resultItem = savedRemoteEventArgs?.Item;
        if (resultItem == null)
            return;
        OnItemRenamed(sender, resultItem, true);
    }

    public void OnItemSaved(object sender, EventArgs args)
    {
        Assert.ArgumentNotNull(sender, nameof(sender));
        Assert.ArgumentNotNull(args, nameof(args));
        var item = Event.ExtractParameter(args, 0) as Item;
        OnItemSaved(sender, item, false);
    }

    public void OnItemSavedRemote(object sender, EventArgs args)
    {
        Assert.ArgumentNotNull(sender, nameof(sender));
        Assert.ArgumentNotNull(args, nameof(args));
        if (args is ItemSavedRemoteEventArgs savedRemoteEventArgs)
        {
            OnItemSaved(sender, savedRemoteEventArgs.Item, true);
        }
    }
}