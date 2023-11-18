namespace SharedCache.Core.Providers
{
    using SharedCache.Core.Extensions;
    using SharedCache.Core.Providers.Keys;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// An implementation of a string cache using a System.Web.SessionState.HttpSessionState as storage.
    /// </summary>
    public class HttpSessionStringCache : StringCache
    {
        private readonly HttpSessionStateBase session;
        private readonly KeyWrapper keyWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStringCache"/> class.
        /// </summary>
        public HttpSessionStringCache(string cacheName, HttpSessionStateBase session)
        {
            this.CacheName = cacheName;
            var keyValidator = new KeyValidator(KeyValidationMode.NoCheck, KeyValidationMode.NoCheck, new char[] {'/'});
            this.keyWrapper = new KeyWrapper(cacheName, null, string.Empty, '/', keyValidator);
            this.session = session;
        }

        #region Strings

        /// <inheritdoc />
        public override string GetString(string key)
        {
            var encodedValue = session[keyWrapper.WrapKey(key, KeyType.str)] as string;
            if (encodedValue == null)
                return null;

            var value = encodedValue.RemoveExpire(out var isExpired);
            if (isExpired)
            {
                RemoveString(key);
                return null;
            }

            return value;
        }

        /// <inheritdoc />
        public override List<string> GetStringKeys()
        {
            return GetPatternKeys<string>(KeyType.str);
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetStrings()
        {
            return GetPatternValues<string>(KeyType.str);
        }

        /// <inheritdoc />
        public override bool SetString(string key, string value)
        {
            return SetString(key, value, null);
        }

        /// <inheritdoc />
        public override bool SetString(string key, string value, TimeSpan expiry)
        {
            return SetString(key, value, expiry as TimeSpan?);
        }

        private bool SetString(string key, string value, TimeSpan? expiry)
        {
            if (!CheckKeyValue(key, value))
                return false;

            var cacheKey = keyWrapper.WrapKey(key, KeyType.str);

            if (session[cacheKey] != null)
            {
                session.Remove(keyWrapper.WrapKey(key, KeyType.str));
            }

            session.Add(cacheKey, value.AddExpire(expiry));

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveString(string key)
        {
            key = keyWrapper.WrapKey(key, KeyType.str);
            session.Remove(key);
            return true;
        }

        #endregion Strings

        #region Lists

        /// <inheritdoc />
        public override List<string> GetList(string key)
        {
            return session[keyWrapper.WrapKey(key, KeyType.lst)] as List<string>;
        }

        /// <inheritdoc />
        public override List<string> GetListKeys()
        {
            return GetPatternKeys<List<string>>(KeyType.lst);
        }

        /// <inheritdoc />
        public override Dictionary<string,List<string>> GetLists()
        {
            return GetPatternValues<List<string>>(KeyType.lst);
        }

        /// <inheritdoc />
        public override bool SetList(string key, IList<string> list)
        {
            if (!CheckKeyValue(key, list))
                return false;

            session[keyWrapper.WrapKey(key, KeyType.lst)] = list;

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveList(string key)
        {
            if (session[keyWrapper.WrapKey(key, KeyType.lst)] != null)
            {
                session.Remove(keyWrapper.WrapKey(key, KeyType.lst));
                return true;
            }

            return false;
        }

        #endregion

        #region Dictionaries

        /// <inheritdoc />
        public override Dictionary<string, string> GetDictionary(string key)
        {
            return session[keyWrapper.WrapKey(key, KeyType.dic)] as Dictionary<string, string>;
        }

        /// <inheritdoc />
        public override List<string> GetDictionaryKeys()
        {
            return GetPatternKeys<Dictionary<string, string>>(KeyType.dic);
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

            session[keyWrapper.WrapKey(key, KeyType.dic)] = dictionary;

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveDictionary(string key)
        {
            if (session[keyWrapper.WrapKey(key, KeyType.dic)] != null)
            {
                session.Remove(keyWrapper.WrapKey(key, KeyType.dic));
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

        private List<string> GetPatternKeys(string pattern)
        {
            List<string> keys = new List<string>();

            foreach (var k in session.Keys)
            {
                var key = k as string;

                if (key == null)
                    continue;

                if (key.StartsWith(pattern))
                    keys.Add(key);
            }

            return keys;
        }

        private List<string> GetPatternKeys<T>(KeyType keyType)
        {
            var pattern = keyWrapper.WrapKeyPattern(keyType);

            List<string> keys = new List<string>();

            foreach (var k in session.Keys.Cast<object>().ToList())
            {
                var wrappedKey = k as string;

                if (wrappedKey == null)
                    continue;

                if (wrappedKey.StartsWith(pattern) && session[wrappedKey] is T)
                {
                    if (typeof(T) == typeof(string))
                        if (!StringExists(wrappedKey))
                            continue;

                    keys.Add(keyWrapper.UnwrapKey(wrappedKey, keyType));
                }
            }

            return keys;
        }

        private Dictionary<string, T> GetPatternValues<T>(KeyType keyType)
        {
            var pattern = keyWrapper.WrapKeyPattern(keyType);

            Dictionary<string, T> values = new Dictionary<string, T>();

            foreach (var k in session.Keys)
            {
                var wrappedKey = k as string;

                if (wrappedKey == null)
                    continue;

                if (wrappedKey.StartsWith(pattern) && session[wrappedKey] is T)
                {
                    if (typeof(T) == typeof(string))
                        if (!StringExists(wrappedKey))
                            continue;

                    values.Add(keyWrapper.UnwrapKey(wrappedKey, keyType), (T)session[wrappedKey]);
                }
            }

            return values;
        }

        private void RemovePattern(string pattern)
        {
            var keys = GetPatternKeys(pattern);
            foreach (var key in keys)
                session.Remove(key);
        }

        private bool StringExists(string wrappedKey)
        {
            var encodedValue = session[wrappedKey] as string;
            if (encodedValue == null)
                return false;

            encodedValue.RemoveExpire(out var isExpired);
            if (isExpired)
            {
                RemoveStringFromWrappedKey(wrappedKey);
                return false;
            }

            return true;
        }

        private bool RemoveStringFromWrappedKey(string wrappedKey)
        {
            session.Remove(wrappedKey);
            return true;
        }
    }
}