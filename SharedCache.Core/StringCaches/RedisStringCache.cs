namespace SharedCache.Core.Providers
{
    using SharedCache.Core.Providers.Keys;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// An implementation of a string cache using a Redis as storage.
    /// </summary>
    public class RedisStringCache : StringCache
    {
        private static IDatabase StaticDatabase;
        private IDatabase Database;
        private readonly KeyWrapper keyWrapper;
        private const char divider = '@';

        private static string defaultConnectionString
        {
            get
            {
                var connection = ConfigurationManager.AppSettings[Settings.RedisConnectionStringName];
                if(string.IsNullOrEmpty(connection))
                    connection = ConfigurationManager.ConnectionStrings[Settings.RedisConnectionStringName]?.ConnectionString;
                if (string.IsNullOrEmpty(connection))
                    throw new ConfigurationErrorsException($"Redis connection string is missing: {Settings.RedisConnectionStringName}");
                return connection;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisStringCache"/> class.
        /// This constructor should only be used for unit testing.
        /// </summary>
        public RedisStringCache(string cacheName, IDatabase database)
        {
            this.CacheName = cacheName;
            var keyValidator = new KeyValidator(KeyValidationMode.NoCheck, KeyValidationMode.NoCheck, new char[] { divider });
            this.keyWrapper = new KeyWrapper(cacheName, null, string.Empty, divider, keyValidator);
            Database = database;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisStringCache"/> class.
        /// This constructor should only be used for unit testing.
        /// </summary>
        public RedisStringCache(string cacheName, string connectionString)
        {
            this.CacheName = cacheName;
            var keyValidator = new KeyValidator(KeyValidationMode.NoCheck, KeyValidationMode.NoCheck, new char[] { divider });
            this.keyWrapper = new KeyWrapper(cacheName, null, string.Empty, divider, keyValidator);
            Database = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString))?.Value?.GetDatabase();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisStringCache"/> class.
        /// </summary>
        public RedisStringCache(string cacheName)
        {
            this.CacheName = cacheName;
            var keyValidator = new KeyValidator(KeyValidationMode.CheckAndFail, KeyValidationMode.CheckAndFail, new char[] { divider });
            this.keyWrapper = new KeyWrapper(cacheName, null, string.Empty, divider, keyValidator);
            if (StaticDatabase == null)
                StaticDatabase = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(defaultConnectionString))?.Value?.GetDatabase();
            Database = StaticDatabase;
        }

        #region Strings

        /// <inheritdoc />
        public override string GetString(string key)
        {
            if (!CheckKey(key))
                return null;

            var value = Database.StringGet(keyWrapper.WrapKey(key, KeyType.str));

            if (!value.HasValue)
            {
                // String is expired
                RemoveKey(key, KeyType.str);
                return null;
            }

            return value.ToString().Substring(1);
        }

        /// <inheritdoc />
        public override List<string> GetStringKeys()
        {
            var keys = GetIndexKeys(KeyType.str).ToArray();
            var batch = Database.CreateBatch();
            var batchResult = new Dictionary<string, Task<RedisValue>>();

            foreach (var key in keys)
            {
                var task = batch.StringGetAsync(keyWrapper.WrapKey(key, KeyType.str));
                batchResult.Add(key, task);
            }

            batch.Execute();
            Task.WhenAll(batchResult.Values);

            // As string can expire, we need this filter
            return batchResult.ToDictionary(kv => kv.Key, kv => kv.Value.Result).Where(kv => kv.Value.HasValue).Select(kv => kv.Key).ToList();
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetStrings()
        {
            var keys = GetIndexKeys(KeyType.str).ToArray();
            var batch = Database.CreateBatch();
            var batchResult = new Dictionary<string, Task<RedisValue>>();

            foreach (var key in keys)
            {
                var task = batch.StringGetAsync(keyWrapper.WrapKey(key, KeyType.str));
                batchResult.Add(key, task);
            }

            batch.Execute();
            Task.WhenAll(batchResult.Values);

            // As string can expire, we need this filter
            return batchResult.ToDictionary(kv => kv.Key, kv => kv.Value.Result).Where(kv => kv.Value.HasValue).ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        }

        /// <inheritdoc />
        public override bool SetString(string key, string value)
        {
            if (!CheckKeyValue(key, value))
                return false;

            // Redis does not support empty string, to we will append a space, and remove it when reading it back
            SetKey(key, KeyType.str, (batch, wrappedKey) => batch.StringSetAsync(wrappedKey, string.Concat(" ", value), null, When.Always, CommandFlags.FireAndForget));

            return true;
        }

        /// <inheritdoc />
        public override bool SetString(string key, string value, TimeSpan expiry)
        {
            if (!CheckKeyValue(key, value))
                return false;

            // Redis does not support empty string, to we will append a space, and remove it when reading it back
            SetKey(key, KeyType.str, (batch, wrappedKey) => batch.StringSetAsync(wrappedKey, string.Concat(" ", value), expiry, When.Always, CommandFlags.FireAndForget));

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveString(string key)
        {
            if (!CheckKey(key))
                return false;

            return RemoveKey(key, KeyType.str);
        }

        #endregion

        #region Lists

        /// <inheritdoc />
        public override List<string> GetList(string key)
        {
            return Database.HashGetAll(keyWrapper.WrapKey(key, KeyType.lst))?.OrderBy(e => int.Parse(e.Name)).Select(e => e.Value.ToString()).ToList();
        }

        /// <inheritdoc />
        public override List<string> GetListKeys()
        {
            return GetIndexKeys(KeyType.lst).ToList();
        }

        /// <inheritdoc />
        public override Dictionary<string, List<string>> GetLists()
        {
            var keys = GetIndexKeys(KeyType.lst).ToArray();
            var batch = Database.CreateBatch();
            var batchResult = new Dictionary<string, Task<HashEntry[]>>();

            foreach(var key in keys)
            {
                var task = batch.HashGetAllAsync(keyWrapper.WrapKey(key, KeyType.lst));
                batchResult.Add(key, task);
            }

            batch.Execute();
            Task.WhenAll(batchResult.Values);

            return batchResult.ToDictionary(kv => kv.Key, kv => kv.Value.Result?.OrderBy(v => int.Parse(v.Name)).Select(e => e.Value.ToString()).ToList());
        }

        /// <inheritdoc />
        public override bool SetList(string key, IList<string> list)
        {
            if (!CheckKeyValue(key, list))
                return false;

            var hashFields = new HashEntry[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                hashFields[i] = new HashEntry(i.ToString(), list[i]);
            }

            SetKey(key, KeyType.lst, (batch, wrappedKey) => batch.HashSetAsync(wrappedKey, hashFields, CommandFlags.FireAndForget));

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveList(string key)
        {
            if (!CheckKey(key))
                return false;

            return RemoveKey(key, KeyType.lst);
        }

        #endregion

        #region Dictionaries

        /// <inheritdoc />
        public override Dictionary<string, string> GetDictionary(string key)
        {
            return Database.HashGetAll(keyWrapper.WrapKey(key, KeyType.dic))?.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
        }

        /// <inheritdoc />
        public override List<string> GetDictionaryKeys()
        {
            return GetIndexKeys(KeyType.dic).ToList();
        }

        /// <inheritdoc />
        public override Dictionary<string, Dictionary<string, string>> GetDictionaries()
        {
            var keys = GetIndexKeys(KeyType.dic).ToArray();
            var batch = Database.CreateBatch();
            var batchResult = new Dictionary<string, Task<HashEntry[]>>();

            foreach (var key in keys)
            {
                var task = batch.HashGetAllAsync(keyWrapper.WrapKey(key, KeyType.dic));
                batchResult.Add(key, task);
            }

            batch.Execute();
            Task.WhenAll(batchResult.Values);

            return batchResult.ToDictionary(kv => kv.Key, kv => kv.Value.Result?.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString()));
        }

        /// <inheritdoc />
        public override bool SetDictionary(string key, IDictionary<string, string> dictionary)
        {
            if (!CheckKeyValue(key, dictionary))
                return false;

            var hashFields = new List<HashEntry>(dictionary.Count);

            foreach (var kv in dictionary)
                hashFields.Add(new HashEntry(kv.Key, kv.Value));

            SetKey(key, KeyType.dic, (batch, wrappedKey) => { batch.HashSetAsync(wrappedKey, hashFields.ToArray(), CommandFlags.FireAndForget); });

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveDictionary(string key)
        {
            if (!CheckKey(key))
                return false;

            return RemoveKey(key, KeyType.dic); 
        }

        /// <inheritdoc />
        public override string GetDictionaryElement(string key, string dictionaryKey)
        {
            var wrappedKey = keyWrapper.WrapKey(key, KeyType.dic);

            if (!Database.KeyExists(wrappedKey))
                return null;

            var value = Database.HashGet(wrappedKey, dictionaryKey);

            return value.HasValue ? value.ToString() : null;
        }

        /// <inheritdoc />
        public override bool SetDictionaryElement(string key, string dictionaryKey, string dictionaryValue)
        {
            if (!CheckKeyValue(key, dictionaryKey, dictionaryValue))
                return false;

            var wrappedKey = keyWrapper.WrapKey(key, KeyType.dic);

            if (!Database.KeyExists(wrappedKey))
                return false;

            Database.HashSet(wrappedKey, new[] { new HashEntry(dictionaryKey, dictionaryValue) }, CommandFlags.FireAndForget);

            return true;
        }

        /// <inheritdoc />
        public override bool RemoveDictionaryElement(string key, string dictionaryKey)
        {
            var wrappedKey = keyWrapper.WrapKey(key, KeyType.dic);

            if (!Database.KeyExists(wrappedKey))
                return false;

            return Database.HashDelete(wrappedKey, dictionaryKey, CommandFlags.FireAndForget);
        }

        #endregion

        /// <inheritdoc />
        public override void Clear()
        {
            var keys = new List<string>();

            foreach(var keyType in new [] {KeyType.str, KeyType.lst, KeyType.dic})
            {
                keys.AddRange(GetIndexKeys(keyType).Select(key => keyWrapper.WrapKey(key, keyType)));
                keys.Add(keyWrapper.WrapKeyPattern(keyType));
            }

            Database.KeyDelete(keys.Select(k => new RedisKey(k)).ToArray(), CommandFlags.FireAndForget);
        }

        /// <inheritdoc />
        public override int Count()
        {
            var count = 0;

            foreach (var keyType in new[] { KeyType.str, KeyType.lst, KeyType.dic })
            {
                count += GetExistingIndexKeys(keyType).Count();
            }

            return count;
        }

        #region Index operations

        private void SetKey(string key, KeyType keyType, Action<IBatch, string> continuation)
        {
            var wrapPattern = keyWrapper.WrapKeyPattern(keyType);
            var wrappedKey = keyWrapper.WrapKey(key, keyType);

            var batch = Database.CreateBatch();
            batch.SetAddAsync(wrapPattern, key, CommandFlags.FireAndForget);
            continuation(batch, wrappedKey);

            batch.Execute();
        }

        private bool RemoveKey(string key, KeyType keyType)
        {
            var wrapPattern = keyWrapper.WrapKeyPattern(keyType);
            var wrappedKey = keyWrapper.WrapKey(key, keyType);

            var batch = Database.CreateBatch();
            var batchResult = new List<Task<bool>>();

            batchResult.Add(batch.SetRemoveAsync(wrapPattern, key));
            batchResult.Add(batch.KeyDeleteAsync(wrappedKey));

            batch.Execute();
            Task.WhenAll(batchResult);

            return batchResult.All(t => t.Result);
        }

        private IEnumerable<string> GetIndexKeys(KeyType keyType)
        {
            return Database.SetMembers(keyWrapper.WrapKeyPattern(keyType)).Select(value => value.ToString());
        }

        private List<string> GetExistingIndexKeys(KeyType keyType)
        {
            var keys = GetIndexKeys(keyType).ToArray();
            var batch = Database.CreateBatch();
            var batchResult = new Dictionary<string, Task<bool>>();

            foreach (var key in keys)
            {
                var task = batch.KeyExistsAsync(keyWrapper.WrapKey(key, keyType));
                batchResult.Add(key, task);
            }

            batch.Execute();
            Task.WhenAll(batchResult.Values);

            return batchResult.ToDictionary(kv => kv.Key, kv => kv.Value.Result).Where(kv => kv.Value).Select(kv => kv.Key).ToList();
        }

        #endregion
    }
}