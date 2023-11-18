namespace SharedCache.Custom.Models
{
    using Sitecore.Data.Items;

    /// <summary>
    /// Interface for a custom cache
    /// </summary>
    public interface ICustomCache
    {
        /// <summary>
        /// Clears the cache for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="clearSecondLevelCache">if set to <c>true</c> [clear shared cache].</param>
        void ClearCacheForItem(Item item, bool clearSecondLevelCache);
    }
}