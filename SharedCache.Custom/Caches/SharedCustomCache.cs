namespace SharedCache.Custom.Caches
{
    using SharedCache.Core.Serialization;
    using SharedCache.Core.Providers;
    using System;
    using SharedCache.Custom.ClearPredicates;

    /// <summary>
    /// An implementation of a shared custom cache. The custom cache support the storage of a generic TValue.
    /// The shared custom cache uses a string cache as a second level cache. The TValue recieved from the cache
    /// is immutable.
    /// </summary>
    public class SharedCustomCache<TValue> : CustomCache<TValue>, IDisposable where TValue : class, new()
    {
        private readonly StringCache secondLevelCache;
        private readonly ICacheSerializer<TValue> valueCacheSerializer;
        private readonly bool clearOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedCustomCache{TValue}"/> class.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        /// <param name="secondLevelCache">The second level cache</param>
        /// <param name="clearPredicate">The clear predicate controlling when and how the cache is cleared.</param>
        /// <param name="valueCacheSerializer">The value cache serializer controlling the serialization of the generic object to the second level cache.</param>
        /// <param name="clearOnly">Whether to put the second level cache in clear only mode.</param>
        public SharedCustomCache(string name, StringCache secondLevelCache, ClearPredicate clearPredicate, ICacheSerializer<TValue> valueCacheSerializer, bool clearOnly) : base(name, clearPredicate)
        {
            this.secondLevelCache = secondLevelCache;
            this.valueCacheSerializer = valueCacheSerializer;
            this.clearOnly = clearOnly;
            Initialize();
        }

        private void Initialize()
        {
            if (secondLevelCache == null || clearOnly)
                return;

            try
            {
                var kvs = secondLevelCache.GetStrings();

                foreach (var kv in kvs)
                    this.SetObject(kv.Key, valueCacheSerializer.Deserialize(kv.Value));

                Log($"Initialized {Name} with {kvs.Count} items", false);
            }
            catch (Exception ex)
            {
                Log($"Initialized {Name} failed", ex);
            }
        }

        /// <inheritdoc/>
        public override TValue Get(string key)
        {
            var value = this.GetObject(key) as TValue;
            if (value != null || secondLevelCache == null || clearOnly)
                return value;

            string sharedCachedStringValue = null;

            try
            {
                sharedCachedStringValue = secondLevelCache.GetString(key);
            }
            catch (Exception ex)
            {
                Log("Could not read from second level cache", ex);
            }

            if (!string.IsNullOrWhiteSpace(sharedCachedStringValue))
            {
                value = valueCacheSerializer.Deserialize(sharedCachedStringValue);
                SetObject(key, value);
            }

            return value;
        }

        /// <inheritdoc/>
        public override void Set(string key, TValue value)
        {
            this.SetObject(key, value);

            if (secondLevelCache == null || clearOnly)
                return;

            try
            {
                secondLevelCache?.SetString(key, valueCacheSerializer.Serialize(value));
            }
            catch (Exception ex)
            {
                Log("Could not write to second level cache", ex);
            }
        }

        /// <inheritdoc/>
        public override void Clear(bool clearSecondLevelCache)
        {
            Log($"Clearing cache{(clearSecondLevelCache ? " including second level cache" : string.Empty)}");
            InnerCache.Clear();

            if (clearSecondLevelCache)
            {
                try
                {
                    this.secondLevelCache?.Clear();
                }
                catch (Exception ex)
                {
                    Log("Could not clear second level cache", ex);
                }
            }
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            Clear(true);
        }

        /// <inheritdoc/>
        public override void Remove(string key, bool clearSecondLevelCache)
        {
            Log($"Removing {key}{(clearSecondLevelCache ? " including second level cache" : string.Empty)}");
            base.Remove(key);

            if (clearSecondLevelCache)
            {
                try
                {
                    this.secondLevelCache?.RemoveString(key);
                }
                catch (Exception ex)
                {
                    Log("Could not remove from second level cache", ex);
                }
            }
        }
    }
}