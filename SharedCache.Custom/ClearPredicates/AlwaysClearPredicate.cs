namespace SharedCache.Custom.ClearPredicates
{
    using Sitecore.Data.Items;
    using Sitecore.Events;

    /// <summary>
    /// Clear on all publishes
    /// </summary>
    public class AlwaysClearPredicate : IClearPredicate
    {
        /// <summary>
        /// Gets or sets a value indicating whether to clear on global publish.
        /// </summary>
        public bool ClearOnGlobal { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlwaysClearPredicate"/> class.
        /// </summary>
        public AlwaysClearPredicate(bool clearOnGlobalPublish)
        {
            this.ClearOnGlobal = clearOnGlobalPublish;
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public bool Execute(SitecoreEventArgs args)
        {
            if (!(args.Parameters[1] is ItemChanges))
                return false;

            return true;
        }
    }
}