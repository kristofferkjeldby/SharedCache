namespace SharedCache.Core.StringCaches
{
    using SharedCache.Core.Extensions;
    using SharedCache.Core.StringCaches.Keys;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// An implementation of a string cache using the filesystem as storage.
    /// </summary>
    public class FileStringCache : StringCache
    {
        private readonly string rootPath;
        private readonly static object _lock = new object();
        private readonly KeyWrapper keyWrapper;
        private const char divider = '-';
        private const char wildcard = '*';
        private const string postfix = ".txt";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStringCache"/> class.
        /// </summary>
        public FileStringCache() : this(nameof(FileStringCache), nameof(FileStringCache))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStringCache"/> class.
        /// </summary>
        /// <param name="cacheName">Name of the cache.</param>
        /// <param name="rootPath">The root path.</param>
        public FileStringCache(string cacheName, string rootPath)
        {
            this.CacheName = cacheName;
            var keyValidator = new KeyValidator(KeyValidationMode.CheckAndFail, KeyValidationMode.CheckAndFail, Path.GetInvalidFileNameChars().Union(new[] { wildcard, divider }));
            this.keyWrapper = new KeyWrapper(cacheName, wildcard, postfix, divider, keyValidator);
            this.rootPath = rootPath;

            if (!Directory.Exists(rootPath))
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(rootPath);
                }
            }
        }

        #region Strings

        /// <inheritdoc/>
        public override string GetString(string key)
        {
            var path = Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.str));

            if (!File.Exists(path))
                return null;
            
            var value = File.ReadAllText(path).RemoveExpire(out var isExpired);

            if (isExpired)
            {
                RemoveString(key);
                return null;
            }

            return value;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetStrings()
        {
            return GetStringKeys().ToDictionary(key => key, key => GetString(key));
        }

        /// <inheritdoc/>
        public override bool SetString(string key, string value)
        {
            return SetString(key, value, null);
        }

        /// <inheritdoc/>
        public override bool SetString(string key, string value, TimeSpan expiry)
        {
            return SetString(key, value, expiry as TimeSpan?);
        }

        private bool SetString(string key, string value, TimeSpan? expiry)
        {
            if (!CheckKeyValue(key, value))
                return false;

            lock (_lock)
            {
                File.WriteAllText(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.str)), value.AddExpire(expiry));
            }

            return true;
        }

        /// <inheritdoc/>
        public override List<string> GetStringKeys()
        {
            return GetPatternFiles(keyWrapper.WrapKeyPattern(KeyType.str)).Select(f => keyWrapper.UnwrapKey(f, KeyType.str)).Where(key => GetString(key) != null).ToList();
        }

        /// <inheritdoc/>
        public override bool RemoveString(string key)
        {
            var wrappedKey = keyWrapper.WrapKey(key, KeyType.str);

            lock (_lock)
            {
                if (!File.Exists(Path.Combine(rootPath, wrappedKey)))
                    return false;
                File.Delete(Path.Combine(rootPath, wrappedKey));
            }

            return true;
        }

        #endregion Strings

        #region Lists

        /// <inheritdoc/>
        public override List<string> GetList(string key)
        {
            if (!File.Exists(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.lst))))
                return null;

            var files = GetPatternFiles(keyWrapper.WrapListElementPattern(key));
            return files.OrderBy(f => keyWrapper.UnwrapListElementKey(f)).Select(f => File.ReadAllText(f)).ToList();
        }

        /// <inheritdoc/>
        public override List<string> GetListKeys()
        {
            return GetPatternFiles(keyWrapper.WrapKeyPattern(KeyType.lst)).Select(f => keyWrapper.UnwrapKey(f, KeyType.lst)).ToList();
        }

        /// <inheritdoc/>
        public override Dictionary<string, List<string>> GetLists()
        {
            return GetListKeys().ToDictionary(key => key, key => GetList(key));
        }

        /// <inheritdoc/>
        public override bool SetList(string key, IList<string> list)
        {
            if (!CheckKeyValue(key, list))
                return false;

            lock (_lock)
            {
                File.WriteAllText(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.lst)), string.Empty);

                for (int i = 0; i < list.Count(); i++)
                    File.WriteAllText(Path.Combine(rootPath, keyWrapper.WrapListElementKey(key, i)), list[i]);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool RemoveList(string key)
        {
            lock (_lock)
            {
                if (!File.Exists(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.lst))))
                    return false;

                File.Delete(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.lst)));

                RemovePatternFiles(keyWrapper.WrapListElementPattern(key));
                return true;
            }
        }

        #endregion;

        #region Dictionary

        /// <inheritdoc/>
        public override Dictionary<string, string> GetDictionary(string key)
        {
            if (!File.Exists(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.dic))))
                return null;

            var files = GetPatternFiles(keyWrapper.GetDictionaryElementPattern(key));
            return files.ToDictionary(f => keyWrapper.UnwrapDictionarytElementKey(f), f => File.ReadAllText(f));
        }

        /// <inheritdoc/>
        public override List<string> GetDictionaryKeys()
        {
            return GetPatternFiles(keyWrapper.WrapKeyPattern(KeyType.dic)).Select(f => keyWrapper.UnwrapKey(f, KeyType.dic)).ToList();
        }

        /// <inheritdoc/>
        public override Dictionary<string, Dictionary<string, string>> GetDictionaries()
        {
            return GetDictionaryKeys().ToDictionary(key => key, key => GetDictionary(key));
        }

        /// <inheritdoc/>
        public override bool SetDictionary(string key, IDictionary<string, string> dictionary)
        {
            if (!CheckKeyValue(key, dictionary))
                return false;

            lock (_lock)
            {
                File.WriteAllText(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.dic)), string.Empty);

                foreach (var kv in dictionary)
                    File.WriteAllText(Path.Combine(rootPath, keyWrapper.WrapDictionaryElementKey(key, kv.Key)), kv.Value);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool RemoveDictionary(string key)
        {
            lock (_lock)
            {
                if (!File.Exists(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.dic))))
                    return false;

                File.Delete(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.dic)));

                RemovePatternFiles(keyWrapper.GetDictionaryElementPattern(key));
                return true;
            }
        }

        /// <inheritdoc/>
        public override string GetDictionaryElement(string key, string dictionaryKey)
        {
            if (!File.Exists(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.dic))))
                return null;

            var path = Path.Combine(rootPath, keyWrapper.WrapDictionaryElementKey(key, dictionaryKey));

            if (!File.Exists(path))
                return null;

            return File.ReadAllText(path);
        }

        /// <inheritdoc/>
        public override bool SetDictionaryElement(string key, string dictionaryKey, string dictionaryValue)
        {
            if (!CheckKeyValue(key, dictionaryKey, dictionaryValue))
                return false;

            if (!File.Exists(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.dic))))
                return false;

            lock (_lock)
            {
                File.WriteAllText(Path.Combine(rootPath, keyWrapper.WrapDictionaryElementKey(key, dictionaryKey)), dictionaryValue);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool RemoveDictionaryElement(string key, string dictionaryKey)
        {
            if (!File.Exists(Path.Combine(rootPath, keyWrapper.WrapKey(key, KeyType.dic))))
                return false;

            var path = Path.Combine(rootPath, keyWrapper.WrapDictionaryElementKey(key, dictionaryKey));

            if (!File.Exists(path))
                return false;

            lock (_lock)
            {
                File.Delete(path);
            }

            return true;
        }

        #endregion

        /// <inheritdoc/>
        public override void Clear()
        {
            lock (_lock)
            {
                RemovePatternFiles(keyWrapper.WrapPattern());
            }
        }

        /// <inheritdoc/>
        public override int Count()
        {
            return GetPatternFiles(keyWrapper.WrapKeyPattern('?')).Count();
        }

        private IEnumerable<string> GetPatternFiles(string pattern)
        {
            return Directory.EnumerateFiles(rootPath, pattern);
        }

        private void RemovePatternFiles(string pattern)
        {
            var keys = GetPatternFiles(pattern);

            foreach (var key in keys)
                File.Delete(key);
        }
    }
}