namespace SharedCache.Custom.ClearPredicates
{
    using Sitecore.Events;

    /// <summary>
    /// Interface for a clear predicate
    /// </summary>
    public interface IClearPredicate
    {
        /// <summary>
        /// Gets or sets a value indicating whether to clear on global publish.
        /// </summary>
        bool ClearOnGlobal { get; }

        /// <summary>
        /// Determines whether to clear the cache.
        /// </summary>
        bool Execute(SitecoreEventArgs args);
    }
}