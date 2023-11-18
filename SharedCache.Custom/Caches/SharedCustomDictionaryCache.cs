namespace SharedCache.Custom.Caches
{
    using SharedCache.Custom.ClearPredicates;
    using SharedCache.Core.StringCaches;
    using SharedCache.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharedCache.Core.Serialization;
    using SharedCache.Core.Helpers;

    /// <summary>
    /// An implementation of a shared custom dictionary cache. The shared cache support the storage of a dictionary of generic 
    /// keys (TKey) and values (TValue). The shared custom dictionary cache uses a string cache as a second level cache.
    /// The dictionary recieved from the cache supports the insertion, updating and deletion of elements.
    /// </summary>
    public class SharedCustomDictionaryCache<TKey, TValue> : CustomCache<SharedCustomCacheDictionary<TKey, TValue>>, IDisposable
    {
        private readonly StringCache secondLevelCache;
        private readonly Func<TKey, string> keySerializer;
        private readonly Func<string, TKey> keyDeserialize;
        private readonly Func<TValue, string> valueSerialize;
        private readonly Func<string, TValue> valueDeserialize;
        private readonly bool clearOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedCustomDictionaryCache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="name">The name of the cache.</param>

        /// <param name="clearPredicate">The clear predicate controlling when and how the cache is cleared.</param>
        /// <param name="keyCacheSerializer">The key cache serializer controlling the serialization of the TKey to the second level cache.</param>
        /// <param name="valueCacheSerializer">The value cache serializer controlling the serialization of the TValue to the second level cache.</param>
        /// <param name="secondLevelCache">The second level cache</param>
        /// <param name="clearOnly">Whether to put the second level cache in clear only mode.</param>
        public SharedCustomDictionaryCache(string name, ClearPredicate clearPredicate, ICacheSerializer<TKey> keyCacheSerializer, ICacheSerializer<TValue> valueCacheSerializer, StringCache secondLevelCache = null, bool? clearOnly = null) : base(name, clearPredicate)
        {
            this.keySerializer = keyCacheSerializer.Serialize;
            this.keyDeserialize = keyCacheSerializer.Deserialize;
            this.valueSerialize = valueCacheSerializer.Serialize;
            this.valueDeserialize = valueCacheSerializer.Deserialize;
            this.secondLevelCache = secondLevelCache ?? StringCacheFactory.GetOrCreateStringCache(name, Sitecore.Configuration.Settings.GetSetting(Constants.SecondLevelSharedCustomCacheMethodSetting));
            this.clearOnly = clearOnly ?? Sitecore.Configuration.Settings.GetBoolSetting(Constants.SharedCustomCacheClearOnlySetting, false);

            Initialize();
        }

        private void Initialize()
        {
            if (secondLevelCache == null || clearOnly)
                return;

            try
            {
                var kvs = secondLevelCache.GetDictionaries();

                foreach (var kv in kvs)
                    this.SetObject(kv.Key, kv.Value.ToDictionary(kvv => keyDeserialize(kvv.Key), kvv => kv.Value));

                Log($"Initialized {Name} with {kvs.Count} items", false);
            }
            catch (Exception ex)
            {
                Log($"Initialized {Name} failed", ex);
            }

        }

        /// <inheritdoc/>
        public override SharedCustomCacheDictionary<TKey, TValue> Get(string key)
        {
            var cachedDictionary = this.GetObject(key) as SharedCustomCacheDictionary<TKey, TValue>;

            if (cachedDictionary != null || secondLevelCache == null || clearOnly)
                return cachedDictionary;

            Dictionary<string, string> sharedCachedStringDictionary = null;

            try
            {
                sharedCachedStringDictionary = secondLevelCache.GetDictionary(key);
            }
            catch (Exception ex)
            {
                Log("Could not read from second level cache", ex);
            }

            if (sharedCachedStringDictionary == null)
                return null;

            var sharedCachedDictionary = sharedCachedStringDictionary.ToDictionary(s => keyDeserialize(s.Key), s => valueDeserialize(s.Value));
            var dictionary = CreateDictionary(key, sharedCachedDictionary);
            this.SetObject(key, dictionary);
            return dictionary;
        }

        /// <inheritdoc/>
        public override SharedCustomCacheDictionary<TKey, TValue> Create(string key)
        {
            return CreateDictionary(key, new Dictionary<TKey, TValue>());
        }

        /// <inheritdoc/>
        public override void Set(string key, SharedCustomCacheDictionary<TKey, TValue> value)
        {
            this.SetObject(key, value);

            if (secondLevelCache == null || clearOnly)
                return;

            try
            {
                secondLevelCache.SetDictionary(key, value.Serialize(keySerializer, valueSerialize));
            }
            catch (Exception ex)
            {
                Log("Could not write to second level cache", ex);
            }
        }

        /// <summary>
        /// Sets the dictionary.
        /// </summary>
        public void Set(string key, IDictionary<TKey, TValue> innerDictionary)
        {
            Set(key, CreateDictionary(key, innerDictionary));
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
                    this.secondLevelCache?.RemoveDictionary(key);
                }
                catch (Exception ex)
                {
                    Log("Could not remove from second level cache", ex);
                }
            }
        }

        private SharedCustomCacheDictionary<TKey, TValue> CreateDictionary(string key, IDictionary<TKey, TValue> innerDictionary)
        {
            Action<TKey, TValue> addElementAction = null;
            Action<TKey> removeElementAction = null;

            if (secondLevelCache != null && !clearOnly)
            {
                addElementAction = (dictionaryKey, dictionaryValue) =>
                {
                    try
                    {
                        secondLevelCache?.SetDictionaryElement(key, keySerializer(dictionaryKey), valueSerialize(dictionaryValue));
                    }
                    catch (Exception ex)
                    {
                        Log("Could not write to second level cache", ex);
                    }
                };

                removeElementAction = (dictionaryKey) =>
                {
                    try
                    {
                        secondLevelCache.RemoveDictionaryElement(key, keySerializer(dictionaryKey));
                    }
                    catch (Exception ex)
                    {
                        Log("Could not remove from second level cache", ex);
                    }
                };
            }

            return new SharedCustomCacheDictionary<TKey, TValue>(
                        innerDictionary,
                        addElementAction,
                        removeElementAction,
                        () => secondLevelCache.RemoveDictionary(key)
            );
        }
    }
}