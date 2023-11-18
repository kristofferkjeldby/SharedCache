namespace SharedCache.Core.Providers
{
    using SharedCache.Core.Extensions;
    using SharedCache.Core.Providers.Keys;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Implementation of a indexed string cache. A indexed string cache is wrapper around another string cache.
    /// The need for an indexed cached provider arises when the allowed keys in another string cache (e.g. file system)
    /// are to restrictive, due to the keys including reserved characters, or the keys are very long. The indexed string cache
    /// mitigate these contrains by storing the keys in a index. For normal access the hashed keys are used, limiting the
    /// length of the keys and the possible characters. 
    /// <seealso cref="StringCache" />
    public class IndexedStringCache : StringCache
    {
        private readonly StringCache stringCache;
        private readonly string indexName;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedStringCache"/> class.
        /// </summary>
        public IndexedStringCache(StringCache stringCache, string indexName = "index")
        {
            this.indexName = indexName;
            this.stringCache = stringCache;
            InitializeIndex();
        }

        #region Strings

        /// <inheritdoc/>
        public override string GetString(string key)
        {
            return stringCache.GetString(Hash(key, KeyType.str));
        }

        /// <inheritdoc/>
        public override List<string> GetStringKeys()
        {
            return stringCache.GetStringKeys()?.Select(k => ResolveIndexKey(k, true)).ToList();
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetStrings()
        {
            return stringCache.GetStrings().ToDictionary(kv => ResolveIndexKey(kv.Key, true), kv => kv.Value);
        }

        /// <inheritdoc/>
        public override bool SetString(string key, string value)
        {
            if (!CheckKeyValue(key, value))
                return false;

            SetIndexKey(key, KeyType.str, indexKey => stringCache.SetString(indexKey, value));

            return true;
        }

        /// <inheritdoc/>
        public override bool SetString(string key, string value, TimeSpan expiry)
        {
            if (!CheckKeyValue(key, value))
                return false;

            SetIndexKey(key, KeyType.str, indexKey => stringCache.SetString(indexKey, value, expiry));

            return true;
        }

        /// <inheritdoc/>
        public override bool RemoveString(string key)
        {
            return RemoveIndexKey(key, KeyType.str, indexKey => stringCache.RemoveString(indexKey));
        }

        #endregion

        #region Lists

        /// <inheritdoc/>
        public override List<string> GetList(string key)
        {
            return stringCache.GetList(Hash(key, KeyType.lst));
        }

        /// <inheritdoc/>
        public override List<string> GetListKeys()
        {
            return stringCache.GetListKeys()?.Select(k => ResolveIndexKey(k)).ToList();
        }

        /// <inheritdoc/>
        public override Dictionary<string, List<string>> GetLists()
        {
            return stringCache.GetLists()?.ToDictionary(kv => ResolveIndexKey(kv.Key), kv => kv.Value);
        }

        /// <inheritdoc/>
        public override bool SetList(string key, IList<string> list)
        {
            if (!CheckKeyValue(key, list))
                return false;

            SetIndexKey(key, KeyType.lst, indexKey => stringCache.SetList(indexKey, list));

            return true;
        }

        /// <inheritdoc/>
        public override bool RemoveList(string key)
        {
            return RemoveIndexKey(key, KeyType.lst, indexKey => stringCache.RemoveList(indexKey));
        }

        #endregion

        #region Dictionaries

        /// <inheritdoc/>
        public override Dictionary<string, string> GetDictionary(string key)
        {
            return stringCache.GetDictionary(Hash(key, KeyType.dic))?.ToDictionary(kv => ResolveIndexKey(kv.Key), kv => kv.Value);
        }

        /// <inheritdoc/>
        public override List<string> GetDictionaryKeys()
        {
            return stringCache.GetDictionaryKeys()?.Where(k => k != indexName)?.Select(k => ResolveIndexKey(k)).ToList();
        }

        /// <inheritdoc/>
        public override Dictionary<string, Dictionary<string, string>> GetDictionaries()
        {
            return GetDictionaryKeys()?.ToDictionary(key => key, key => GetDictionary(key));
        }

        /// <inheritdoc/>
        public override bool SetDictionary(string key, IDictionary<string, string> dictionary)
        {
            if (!CheckKeyValue(key, dictionary))
                return false;

            SetIndexKey(key, KeyType.dic, indexKey => stringCache.SetDictionary(indexKey, dictionary.ToDictionary(kv => SetIndexKey(kv.Key, KeyType.dicx), kv => kv.Value)));

            return true;
        }

        /// <inheritdoc/>
        public override bool RemoveDictionary(string key)
        {
            return RemoveIndexKey(key, KeyType.dic, hashedKey => stringCache.RemoveDictionary(hashedKey));
        }

        /// <inheritdoc/>
        public override string GetDictionaryElement(string key, string dictionaryKey)
        {
            return stringCache.GetDictionaryElement(Hash(key, KeyType.dic), Hash(dictionaryKey, KeyType.dicx));
        }

        /// <inheritdoc/>
        public override bool SetDictionaryElement(string key, string dictionaryKey, string dictionaryValue)
        {
            if (!CheckKeyValue(key, dictionaryKey, dictionaryValue))
                return false;

            return SetIndexKey(dictionaryKey, KeyType.dicx, indexKey => stringCache.SetDictionaryElement(Hash(key, KeyType.dic), indexKey, dictionaryValue));
        }

        /// <inheritdoc/>
        public override bool RemoveDictionaryElement(string key, string dictionaryKey)
        {
            return RemoveIndexKey(dictionaryKey, KeyType.dicx, indexKey => stringCache.RemoveDictionaryElement(Hash(key, KeyType.dic), indexKey));
        }

        #endregion

        /// <inheritdoc/>
        public override void Clear()
        {
            stringCache.Clear();
            InitializeIndex();
        }

        /// <inheritdoc/>
        public override int Count()
        {
            return stringCache.Count() - 1;
        }

        #region Index operations

        private string ResolveIndexKey(string hash, bool deleteIfNotExits = false)
        {
            var key = stringCache.GetDictionaryElement(indexName, hash);
            if (key == null && deleteIfNotExits)
                stringCache.RemoveDictionaryElement(indexName, hash);
            return key;
        }

        private string SetIndexKey(string key, KeyType keyType)
        {
            var indexKey = Hash(key, keyType);
            if (stringCache.SetDictionaryElement(indexName, indexKey, key))
                return indexKey;
            return null;
        }

        private bool SetIndexKey(string key, KeyType keyType, Func<string, bool> continuation)
        {
            var indexKey = Hash(key, keyType);
            if (stringCache.SetDictionaryElement(indexName, indexKey, key))
                return continuation(indexKey);
            return false;
        }

        private bool RemoveIndexKey(string key, KeyType keyType, Func<string, bool> continuation)
        {
            var indexKey = Hash(key, keyType);
            if (stringCache.RemoveDictionaryElement(indexName, indexKey))
                return continuation(indexKey);
            return false;
        }

        #endregion

        private string Hash(string key, KeyType keyType)
        {
            return (keyType.ToString() + key).Hash().Substring(0, 30);
        }

        private void InitializeIndex()
        {
            if (stringCache.GetDictionary(indexName) == null)
            {
                stringCache.SetDictionary(indexName, new Dictionary<string, string>());
            }
        }
    }
}