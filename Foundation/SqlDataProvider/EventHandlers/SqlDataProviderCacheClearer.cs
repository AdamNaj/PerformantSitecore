using System;

using PerformantSitecore.Foundation.SqlDataProvider.Providers;

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;

namespace PerformantSitecore.Foundation.SqlDataProvider.EventHandlers;

/// <summary>
/// Clears the CachingSqlDataProvider cache when items are
/// saved, deleted, copied, or otherwise modified - both locally
/// and via remote events in multi-instance deployments.
/// </summary>
public class SqlDataProviderCacheClearer
{
    public void OnItemSaved(object sender, EventArgs args)
    {
        ClearCacheFromItemEvent(args);
    }

    public void OnItemSavedRemote(object sender, EventArgs args)
    {
        ClearCacheFromRemoteEvent(args);
    }

    public void OnItemDeleted(object sender, EventArgs args)
    {
        ClearCacheFromItemEvent(args);
    }

    public void OnItemDeletedRemote(object sender, EventArgs args)
    {
        ClearCacheFromRemoteEvent(args);
    }

    public void OnItemCopied(object sender, EventArgs args)
    {
        ClearCacheFromItemEvent(args);
    }

    public void OnItemCopiedRemote(object sender, EventArgs args)
    {
        ClearCacheFromRemoteEvent(args);
    }

    private static void ClearCacheFromItemEvent(EventArgs args)
    {
        var item = Event.ExtractParameter<Item>(args, 0);
        if (item != null)
        {
            CachingSqlDataProvider.ClearCache(item.Database.Name);
        }
    }

    private static void ClearCacheFromRemoteEvent(EventArgs args)
    {
        var eventArgs = args as SitecoreEventArgs;
        if (eventArgs?.Parameters?.Length > 0)
        {
            // Remote events pass the database name differently depending
            // on the event type. Try to extract a usable database name.
            var item = eventArgs.Parameters[0] as Item;
            if (item != null)
            {
                CachingSqlDataProvider.ClearCache(item.Database.Name);
                return;
            }

            // For some remote events the first parameter is a wrapper.
            // Attempt to get the database name from the event itself.
            try
            {
                var remoteEventArg = eventArgs.Parameters[0];
                var databaseNameProp = remoteEventArg?.GetType().GetProperty("DatabaseName");
                if (databaseNameProp != null)
                {
                    var dbName = databaseNameProp.GetValue(remoteEventArg) as string;
                    if (!string.IsNullOrEmpty(dbName))
                    {
                        CachingSqlDataProvider.ClearCache(dbName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("CachingSqlDataProvider: Could not extract database name from remote event", ex, typeof(SqlDataProviderCacheClearer));
            }
        }
    }
}