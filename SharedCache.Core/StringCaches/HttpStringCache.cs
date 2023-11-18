namespace SharedCache.Core.Providers
{
    using SharedCache.Core.Providers.Keys;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;

    /// <summary>
    /// An implementation of a string cache using a System.Web.Cache as storage.
    /// </summary>
    public class HttpStringCache : StringCache
    {
        private readonly Cache cache;
        private readonly KeyWrapper keyWrapper;
        private const char divider = '@';

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStringCache"/> class.
        /// </summary>
        public HttpStringCache() : this(nameof(HttpStringCache), HttpContext.Current?.Cache)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStringCache"/> class.
        /// </summary>
        public HttpStringCache(string cacheName, Cache cache)
        {
            this.CacheName = cacheName;
            var keyValidator = new KeyValidator(KeyValidationMode.NoCheck, KeyValidationMode.NoCheck, new char[] { divider  });
            this.keyWrapper = new KeyWrapper(cacheName, null, string.Empty, divider, keyValidator);
            this.cache = cache;
        }

        #region Strings

        /// <inheritdoc />
        public override string GetString(string key)
        {
            return cache.Get(keyWrapper.WrapKey(key, KeyType.str)) as string;
        }

        /// <inheritdoc />
        public override List<string> GetStringKeys()
        {
            return GetPatternKeys<string>(KeyType.str).Select(wrappedKey => keyWrapper.UnwrapKey(wrappedKey, KeyType.str)).ToList();
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetStrings()
        {
            return GetPatternValues<string>(KeyType.str);
        }

        /// <inheritdoc />
        public override bool SetString(string key, string value)
        {
            // The Http cache must have an expiry
            var expiry = TimeSpan.FromHours(24);
            return SetString(key, value, expiry);
        }

        /// <inheritdoc />
        public override bool SetString(string key, string value, TimeSpan expiry)
        {
            if (!CheckKeyValue(key, value))
                return false;

            var cacheKey = keyWrapper.WrapKey(key, KeyType.str);

            if (cache.Get(cacheKey) != null)
            {
                cache.Remove(keyWrapper.WrapKey(key, KeyType.str));
            }

            cache.Add(cacheKey, value, null, DateTime.Now.Add(expiry), TimeSpan.Zero, CacheItemPriority.Normal, null);

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveString(string key)
        {
            key = keyWrapper.WrapKey(key, KeyType.str);
            cache.Remove(key);
            return true;
        }

        #endregion Strings

        #region Lists

        /// <inheritdoc />
        public override List<string> GetList(string key)
        {
            return cache[keyWrapper.WrapKey(key, KeyType.lst)] as List<string>;
        }

        /// <inheritdoc />
        public override List<string> GetListKeys()
        {
            return GetPatternKeys<List<string>>(KeyType.lst).Select(wrappedKey => keyWrapper.UnwrapKey(wrappedKey, KeyType.lst)).ToList();
        }

        /// <inheritdoc />
        public override Dictionary<string, List<string>> GetLists()
        {
            return GetPatternValues<List<string>>(KeyType.lst);
        }

        /// <inheritdoc />
        public override bool SetList(string key, IList<string> list)
        {
            if (!CheckKeyValue(key, list))
                return false;

            cache[keyWrapper.WrapKey(key, KeyType.lst)] = list;

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveList(string key)
        {
            if (cache.Get(keyWrapper.WrapKey(key, KeyType.lst)) != null)
            {
                cache.Remove(keyWrapper.WrapKey(key, KeyType.lst));
                return true;
            }

            return false;
        }

        #endregion

        #region Dictionaries

        /// <inheritdoc />
        public override Dictionary<string, string> GetDictionary(string key)
        {
            return cache[keyWrapper.WrapKey(key, KeyType.dic)] as Dictionary<string, string>;
        }

        /// <inheritdoc />
        public override List<string> GetDictionaryKeys()
        {
            return GetPatternKeys<Dictionary<string, string>>(KeyType.dic).Select(wrappedKey => keyWrapper.UnwrapKey(wrappedKey, KeyType.dic)).ToList();
        }

        /// <inheritdoc />
        public override Dictionary<string, Dictionary<string, string>> GetDictionaries()
        {
            return GetPatternValues<Dictionary<string, string>>(KeyType.dic);
        }

        /// <inheritdoc />
        public override bool SetDictionary(string key, IDictionary<string, string> dictionary)
        {
            if (!CheckKeyValue(key, dictionary))
                return false;

            cache[keyWrapper.WrapKey(key, KeyType.dic)] = dictionary;

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveDictionary(string key)
        {
            if (cache.Get(keyWrapper.WrapKey(key, KeyType.dic)) != null)
            {
                cache.Remove(keyWrapper.WrapKey(key, KeyType.dic));
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override string GetDictionaryElement(string key, string dictionaryKey)
        {
            var dictionary = GetDictionary(key);
            if (dictionary == null || !dictionary.ContainsKey(dictionaryKey))
                return null;
            return dictionary[dictionaryKey];
        }

        /// <inheritdoc />
        public override bool SetDictionaryElement(string key, string dictionaryKey, string dictionaryValue)
        {
            if (!CheckKeyValue(key, dictionaryKey, dictionaryValue))
                return false;

            var dictionary = GetDictionary(key);
            if (dictionary == null)
                return false;
            dictionary[dictionaryKey] = dictionaryValue;
            return true;
        }

        /// <inheritdoc />
        public override bool RemoveDictionaryElement(string key, string dictionaryKey)
        {
            var dictionary = GetDictionary(key);
            if (dictionary == null)
                return false;
            return dictionary.Remove(key);
        }

        #endregion

        /// <inheritdoc />
        public override void Clear()
        {
            RemovePattern(keyWrapper.WrapPattern());
        }

        /// <inheritdoc />
        public override int Count()
        {
            return GetPatternKeys(keyWrapper.WrapPattern()).Count();
        }

        private IEnumerable<string> GetPatternKeys(string pattern)
        {
            List<string> keys = new List<string>();

            foreach (DictionaryEntry kv in cache)
            {
                var key = kv.Key as string;

                if (key == null)
                    continue;

                if (key.StartsWith(pattern))
                    keys.Add(key);
            }

            return keys;
        }

        private IEnumerable<string> GetPatternKeys<T>(KeyType keyType)
        {
            return GetPatternKeys(keyWrapper.WrapKeyPattern(keyType)).Where(key => cache[key] is T);
        }

        private Dictionary<string, T> GetPatternValues<T>(KeyType keyType)
        {
            return GetPatternKeys<T>(keyType).ToDictionary(wrappedKey => keyWrapper.UnwrapKey(wrappedKey, keyType), wrappedKey => (T)cache[wrappedKey]);
        }

        private void RemovePattern(string pattern)
        {
            var keys = GetPatternKeys(pattern);
            foreach (var key in keys)
                cache.Remove(key);
        }
    }
}