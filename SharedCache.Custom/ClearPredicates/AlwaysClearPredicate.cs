namespace SharedCache.Custom.ClearPredicates
{
    using Sitecore.Data.Items;

    /// <summary>
    /// Clear on all publishes
    /// </summary>
    public class AlwaysClearPredicate : ClearPredicate
    {
        /// <inheritdoc/>
        public override bool ClearOnGlobal { get; }

        /// <inheritdoc/>
        public override bool UseSiteNameAsCacheKey { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlwaysClearPredicate"/> class.
        /// </summary>
        public AlwaysClearPredicate(bool clearOnGlobalPublish, bool useSiteNameAsCacheKey)
        {
            this.ClearOnGlobal = clearOnGlobalPublish;
            this.UseSiteNameAsCacheKey = useSiteNameAsCacheKey;
        }

        /// <inheritdoc/>
        public override bool DoClear(Item item)
        {
            return true;
        }
    }
}