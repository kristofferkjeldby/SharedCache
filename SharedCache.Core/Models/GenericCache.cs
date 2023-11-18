namespace SharedCache.Core.Models
{
    using Sitecore.Caching;
    using Sitecore.Caching.Generics;
    using Sitecore.Data;
    using Sitecore.Diagnostics.PerformanceCounters;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for a generic cache, that allow registration in Sitecore Cache Manager
    /// </summary>
    public class GenericCache : CustomCache, ICache<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericCache"/> class.
        /// </summary>
        /// <param name="innerCache">The inner cache.</param>
        public GenericCache(ICache innerCache) : base(innerCache)
        {
        }

        #region ICacheInfo

        /// <inheritdoc />
        public int Count => InnerCache.Count;

        /// <inheritdoc />
        public ID Id => InnerCache.Id;

        /// <inheritdoc />
        public long MaxSize { get => InnerCache.MaxSize; set => InnerCache.MaxSize = value; }

        /// <inheritdoc />
        public long RemainingSpace => InnerCache.RemainingSpace;

        /// <inheritdoc />
        public bool Scavengable { get => InnerCache.Scavengable; set => InnerCache.Scavengable = value; }

        /// <inheritdoc />
        public long Size => InnerCache.Size;

        /// <inheritdoc />
        public AmountPerSecondCounter ExternalCacheClearingsCounter { get => InnerCache.ExternalCacheClearingsCounter; set => InnerCache.ExternalCacheClearingsCounter = value; }

        /// <inheritdoc />
        public void Scavenge()
        {
            InnerCache.Scavenge();
        }

        #endregion

        #region ICache

        /// <inheritdoc />
        public void Add(string key, object data)
        {
            InnerCache.Add(key, data);
        }

        /// <inheritdoc />
        public void Add(string key, object data, TimeSpan slidingExpiration)
        {
            InnerCache.Add(key, data, slidingExpiration);
        }

        /// <inheritdoc />
        public void Add(string key, object data, DateTime absoluteExpiration)
        {
            InnerCache.Add(key, data, absoluteExpiration);
        }

        /// <inheritdoc />
        public void Add(string key, object data, EventHandler<EntryRemovedEventArgs<string>> removedHandler)
        {
            InnerCache.Add(key, data, removedHandler);
        }

        /// <inheritdoc />
        public void Add(string key, object data, TimeSpan slidingExpiration, DateTime absoluteExpiration)
        {
            InnerCache.Add(key, data, absoluteExpiration);
        }

        /// <inheritdoc />
        public void Add(string key, object data, TimeSpan slidingExpiration, DateTime absoluteExpiration, EventHandler<EntryRemovedEventArgs<string>> removedHandler)
        {
            InnerCache.Add(key, data, slidingExpiration, absoluteExpiration, removedHandler);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return InnerCache.ContainsKey(key);
        }

        /// <inheritdoc />
        public string[] GetCacheKeys()
        {
            return InnerCache.GetCacheKeys();
        }

        /// <inheritdoc />
        public object GetValue(string key)
        {
            return InnerCache.GetValue(key);
        }

        /// <inheritdoc />
        public ICacheSizeCalculationStrategy CacheSizeCalculationStrategy => InnerCache.CacheSizeCalculationStrategy;

        /// <inheritdoc />
        public object this[string key] => InnerCache[key];

        /// <inheritdoc />
        ICollection<string> ICache<string>.Remove(Predicate<string> predicate)
        {
            return InnerCache.Remove(predicate);
        }

        #endregion
    }
}