namespace SharedCache.Custom.ClearPredicates
{
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Events;
    using Sitecore.Data.Items;
    using Sitecore.Events;

    /// <summary>
    /// Abstract class for a clear predicate
    /// </summary>
    public abstract class ClearPredicate
    {
        /// <summary>
        /// Gets or sets a value indicating whether to clear on global publish.
        /// </summary>
        public abstract bool ClearOnGlobal { get; }

        /// <summary>
        /// Gets a value indicating whether use site name as cache key
        /// </summary>
        public abstract bool UseSiteNameAsCacheKey { get; } 

        /// <summary>
        /// Determines whether to clear the cache.
        /// </summary>
        public bool DoClear(SitecoreEventArgs args)
        {
            if (!(args.Parameters[1] is ItemChanges itemChanges))
                return false;

            var item = itemChanges.Item;

            return DoClear(item);
        }

        /// <summary>
        /// Determines whether to clear the cache.
        /// </summary>
        public bool DoClear(PublishEndRemoteEventArgs args)
        {
            if (!(args is PublishEndRemoteEventArgs sitecoreArgs))
                return false;

            var item = Factory.GetDatabase(sitecoreArgs.TargetDatabaseName)?.GetItem(new ID(sitecoreArgs.RootItemId));

            return DoClear(item);
        }

        /// <summary>
        /// Determines whether to clear the cache.
        /// </summary>
        public abstract bool DoClear(Item item);
    }
}