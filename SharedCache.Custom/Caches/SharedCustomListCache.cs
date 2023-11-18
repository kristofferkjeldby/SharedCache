namespace SharedCache.Custom.Caches
{
    using SharedCache.Core.Providers;
    using SharedCache.Core.Serialization;
    using SharedCache.Custom.ClearPredicates;
    using SharedCache.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An implementation of a shared custom list cache. The custom shared list cache support the storage of a list of generic TValues for each site.
    /// The shared custom list cache uses a string cache as a second level cache. The list recieved from the cache is immutable.
    /// </summary>
    public class SharedCustomListCache<TValue> : CustomCache<SharedCustomCacheList<TValue>>, IDisposable
    {
        private readonly StringCache secondLevelCache;
        private readonly Func<TValue, string> valueSerialize;
        private readonly Func<string, TValue> valueDeserialize;
        private readonly bool clearOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedCustomListCache{TValue}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="secondLevelCache">The second level cache.</param>
        /// <param name="clearPredicate">The clear predicate.</param>
        /// <param name="valueCacheSerializer">The value cache serializer controlling the serialization of the generic object to the second level cache.</param>
        /// <param name="clearOnly">Whether to put the second level cache in clear only mode.</param>
        public SharedCustomListCache(string name, StringCache secondLevelCache, ClearPredicate clearPredicate, ICacheSerializer<TValue> valueCacheSerializer, bool clearOnly) : base(name, clearPredicate)
        {
            this.secondLevelCache = secondLevelCache;
            this.valueSerialize = valueCacheSerializer.Serialize;
            this.valueDeserialize = valueCacheSerializer.Deserialize;
            this.clearOnly = clearOnly;

            Initialize();
        }

        private void Initialize()
        {
            if (secondLevelCache == null || clearOnly)
                return;

            try
            {
                var kvs = secondLevelCache.GetLists();

                foreach (var kv in kvs)
                    this.SetObject(kv.Key, kv.Value.Select(v => valueDeserialize(v)));

                Log($"Initialized {Name} with {kvs.Count} items", false);
            }
            catch (Exception ex)
            {
                Log($"Initialized {Name} failed", ex);
            }

        }

        /// <inheritdoc/>
        public override SharedCustomCacheList<TValue> Get(string key)
        {
            var cachedList = this.GetObject(key) as SharedCustomCacheList<TValue>;
            if (cachedList != null || secondLevelCache == null || clearOnly)
                return cachedList;

            List<string> sharedCachedStringList = null;

            try
            {
                sharedCachedStringList = secondLevelCache.GetList(key);
            }
            catch (Exception ex)
            {
                Log("Could not read from second level cache", ex);
            }

            if (sharedCachedStringList == null)
                return null;

            var sharedCachedList = sharedCachedStringList.Select(s => valueDeserialize(s)).ToList();
            var list = CreateList(key, sharedCachedList);
            this.SetObject(key, list);
            return list;
        }

        /// <inheritdoc/>
        public override SharedCustomCacheList<TValue> Create(string key)
        {
            return CreateList(key, new List<TValue>());
        }

        /// <inheritdoc/>
        public override void Set(string key, SharedCustomCacheList<TValue> list)
        {
            base.SetObject(key, list);

            if (secondLevelCache == null || clearOnly)
                return;

            try
            {
                secondLevelCache.SetList(key, list.Serialize(valueSerialize));
            }
            catch (Exception ex)
            {
                Log("Could not write to second level cache", ex);
            }
        }

        /// <summary>
        /// Sets the list.
        /// </summary>
        public void Set(string key, IList<TValue> innerList)
        {
            Set(key, CreateList(key, innerList));
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
                    this.secondLevelCache?.RemoveList(key);
                }
                catch (Exception ex)
                {
                    Log("Could not remove from second level cache", ex);
                }
            }                
        }

        private SharedCustomCacheList<TValue> CreateList(string key, IList<TValue> innerList)
        {
            return new SharedCustomCacheList<TValue>(innerList);
        }
    }
}