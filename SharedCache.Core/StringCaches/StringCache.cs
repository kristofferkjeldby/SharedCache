namespace SharedCache.Core.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Abstact class for a string cache. A string cache is an simple storage capable of storing string, lists of strings and
    /// dictionaries of string. Strings and lists of strings are immutable, but dictionaries allow the insertion and removal 
    /// of key/value pairs.
    /// </summary>
    public abstract class StringCache
    {
        /// <summary>
        /// Gets or sets a value indicating whether [disable logging].
        /// </summary>
        public bool DisableLogging { get; set; }

        /// <summary>
        /// Gets the cache name.
        /// </summary>
        public string CacheName { get; protected set; }

        #region Strings

        /// <summary>
        /// Get a string
        /// </summary>
        public abstract string GetString(string key);

        /// <summary>
        /// Get all strings keys from cache
        /// </summary>
        public abstract List<string> GetStringKeys();

        /// <summary>
        /// Get all strings from cache
        /// </summary>
        public abstract Dictionary<string, string> GetStrings();

        /// <summary>
        /// Adds or updates a string
        /// </summary>
        public abstract bool SetString(string key, string value);

        /// <summary>
        /// Adds or updates a string
        /// </summary>
        public abstract bool SetString(string key, string value, TimeSpan expiry);

        /// <summary>
        /// Removes a string
        /// </summary>
        public abstract bool RemoveString(string key);

        #endregion

        #region Lists

        /// <summary>
        /// Gets a list.
        /// </summary>
        public abstract List<string> GetList(string key);

        /// <summary>
        /// Gets all list keys in cache.
        /// </summary>
        public abstract List<string> GetListKeys();

        /// <summary>
        /// Gets all lists in cache.
        /// </summary>
        public abstract Dictionary<string, List<string>> GetLists();

        /// <summary>
        /// Creates or update a list.
        /// </summary>>
        public abstract bool SetList(string key, IList<string> list);

        /// <summary>
        /// Removes a list.
        /// </summary>
        public abstract bool RemoveList(string key);

        #endregion;

        #region Dictionaries

        /// <summary>
        /// Gets a dictionary.
        /// </summary>
        public abstract Dictionary<string, string> GetDictionary(string key);

        /// <summary>
        /// Gets all dictionary keys in cache.
        /// </summary>
        public abstract List<string> GetDictionaryKeys();

        /// <summary>
        /// Gets all dictionaries in cache.
        /// </summary>
        public abstract Dictionary<string, Dictionary<string, string>> GetDictionaries();

        /// <summary>
        /// Creates or update a dictionary.
        /// </summary>
        public abstract bool SetDictionary(string key, IDictionary<string, string> dictionary);

        /// <summary>
        /// Removes a dictionary.
        /// </summary>
        public abstract bool RemoveDictionary(string key);

        /// <summary>
        /// Gets a dictionary element.
        /// </summary>
        public abstract string GetDictionaryElement(string key, string dictionaryKey);

        /// <summary>
        /// Creates or updates a dictionary element.
        /// </summary>
        public abstract bool SetDictionaryElement(string key, string dictionaryKey, string dictionaryValue);

        /// <summary>
        /// Removes a dictionary element.
        /// </summary>
        public abstract bool RemoveDictionaryElement(string key, string dictionaryKey);

        #endregion

        /// <summary>
        /// Clears this cache
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Number of entries in cache
        /// </summary>
        public abstract int Count();

        /// <summary>
        /// Checks a key.
        /// </summary>
        protected bool CheckKey(string key)
        {
            if (key == null)
            {
                if (!DisableLogging)
                    Sitecore.Diagnostics.Log.Warn($"[{CacheName}]: Null key added to cache", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks a value.
        /// </summary>
        protected bool CheckValue(string value)
        {
            if (value == null)
            {
                if (!DisableLogging)
                    Sitecore.Diagnostics.Log.Warn($"[{CacheName}]: Null value added to cache", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks a list.
        /// </summary>
        protected bool CheckValue(IList<string> list)
        {
            if (list == null)
            {
                if (!DisableLogging)
                    Sitecore.Diagnostics.Log.Warn($"[{CacheName}]: Null list added to cache", this);
                return false;
            }

            if (list.Any(k => k == null))
            {
                if (!DisableLogging)
                    Sitecore.Diagnostics.Log.Warn($"[{CacheName}]: List containing null keys/values added to cache", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks a dictionary.
        /// </summary>
        protected bool CheckValue(IDictionary<string, string> dictionary)
        {
            if (dictionary == null)
            {
                if (!DisableLogging)
                    Sitecore.Diagnostics.Log.Warn($"[{CacheName}]: Null dictionary added to cache", this);
                return false;
            }

            if (dictionary.Keys.Any(k => k == null) || dictionary.Values.Any(v => v == null))
            {
                if (!DisableLogging)
                    Sitecore.Diagnostics.Log.Warn($"[{CacheName}]: Dictionary containing null keys/values added to cache", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks a dictionary key and value.
        /// </summary>
        protected bool CheckValue(string dictionaryKey, string dictionaryValue)
        {
            if (dictionaryKey == null || dictionaryValue == null)
            {
                if (!DisableLogging)
                    Sitecore.Diagnostics.Log.Warn($"[{CacheName}]: Invalid key/value added to cache", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks a key and a value
        /// </summary>
        protected bool CheckKeyValue(string key, string value)
        {
            return CheckKey(key) && CheckValue(value);
        }

        /// <summary>
        /// Checks a key and a list
        /// </summary>
        protected bool CheckKeyValue(string key, IList<string> list)
        {
            return CheckKey(key) && CheckValue(list);
        }

        /// <summary>
        /// Checks a key and a dictionary
        /// </summary>
        protected bool CheckKeyValue(string key, IDictionary<string, string> dictionary)
        {
            return CheckKey(key) && CheckValue(dictionary);
        }

        /// <summary>
        /// Checks a key and a dictionary key and value
        /// </summary>
        protected bool CheckKeyValue(string key, string dictionaryKey, string dictionaryValue)
        {
            return CheckKey(key) && CheckValue(dictionaryKey, dictionaryValue);
        }
    }
}
