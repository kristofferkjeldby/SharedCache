namespace SharedCache.Html.Helpers
{
    using SharedCache.Html.Models;
    using Sitecore.Data;
    using Sitecore.Data.Events;
    using Sitecore.Data.Items;
    using Sitecore.Events;
    using Sitecore.Publishing;
    using System;
    using System.Linq;

    /// <summary>
    /// Helper class for Sitecore events
    /// </summary>
    public static class EventHelper
    {
        /// <summary>
        /// Gets the publishing information.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <param name="rootItem">The root item.</param>
        /// <param name="clearHtmlCacheFlag">The clear HTML cache flag.</param>
        public static void GetPublishingInfo(EventArgs args, out Item rootItem, out ClearHtmlCacheFlag clearHtmlCacheFlag)
        {
            rootItem = null;
            clearHtmlCacheFlag = ClearHtmlCacheFlag.Both;

            // Handle local event
            if (args is SitecoreEventArgs eventArgs)
            {
                if (eventArgs.Parameters == null || !eventArgs.Parameters.Any())
                {
                    return;
                }

                var publisher = eventArgs.Parameters[0] as Publisher;

                var publishOptions = publisher?.Options;
                rootItem = publishOptions?.RootItem;

                if (publishOptions == null || rootItem == null)
                    return;

                clearHtmlCacheFlag = ClearHtmlCacheFlagEncoder.Decode(publishOptions.PublishDate);
                return;
            }

            // Handle remote events
            if (args is PublishEndRemoteEventArgs remoteEventArgs)
            {
                if (remoteEventArgs == null)
                    return;

                if (string.IsNullOrWhiteSpace(remoteEventArgs.TargetDatabaseName))
                    return;

                var database = Database.GetDatabase(remoteEventArgs.TargetDatabaseName);
                    rootItem = database?.GetItem(new ID(remoteEventArgs.RootItemId));

                clearHtmlCacheFlag = ClearHtmlCacheFlagEncoder.Decode(remoteEventArgs.PublishDate);
                return;
            }
        }
    }
}