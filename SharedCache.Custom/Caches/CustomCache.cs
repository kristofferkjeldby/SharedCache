namespace SharedCache.Custom.Caches
{
    using SharedCache.Core.Models;
    using SharedCache.Custom.ClearPredicates;
    using SharedCache.Custom.Extensions;
    using SharedCache.Custom.Models;
    using Sitecore;
    using Sitecore.Caching;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Events;
    using Sitecore.Data.Items;
    using Sitecore.Events;
    using Sitecore.Publishing;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An implementation of a custom cache. The customer cache support the storage of a generic object for each key, but this object is kept in memory only
    /// </summary>
    public class CustomCache<T> : GenericCache, IDisposable, ICustomCache where T : class, new()
    {
        private readonly object cacheLock = new object();

        private readonly IClearPredicate clearPredicate;

        /// <summary>
        /// The event dictionary
        /// </summary>
        protected Dictionary<string, EventHandler> Events = new Dictionary<string, EventHandler>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomCache{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="clearPredicate">The clear predicate.</param>
        public CustomCache(string name, IClearPredicate clearPredicate) : base((CacheManager.FindCacheByName<string>(name) as GenericCache)?.InnerCache ?? CacheManager.GetNamedInstance(name, StringUtil.ParseSizeString("10MB"), false))
        {
            this.clearPredicate = clearPredicate;

            this.Events.Add("publish:end", this.OnPublishEnd);
            this.Events.Add("publish:end:remote", this.OnPublishEndRemote);
            this.Events.Add("item:saved", this.OnItemSaved);

            foreach (var ev in this.Events)
                Event.Subscribe(ev.Key, ev.Value);

            CacheManager.Register(this);

        }

        #region Event handling

        /// <summary>
        /// Called when item is saved.
        /// </summary>
        public virtual void OnItemSaved(object sender, EventArgs eventArgs)
        {
            if (!(eventArgs is SitecoreEventArgs sitecoreArgs) || sitecoreArgs.Parameters.Length < 2)
                return;

            if (!this.clearPredicate.Execute(sitecoreArgs))
                return;

            this.ClearCacheForItem((sitecoreArgs.Parameters[1] as ItemChanges)?.Item, false);
        }

        /// <summary>
        /// Called on publish end
        /// </summary>
        public void OnPublishEnd(object sender, EventArgs args)
        {
            if (!(args is SitecoreEventArgs sitecoreArgs))
                return;

            var publisher = sitecoreArgs.Parameters[0] as Publisher;

            this.ClearCacheForItem(publisher?.Options.RootItem, true);
        }

        /// <summary>
        /// Called on publish end remote
        /// </summary>
        public void OnPublishEndRemote(object sender, EventArgs args)
        {
            if (!(args is PublishEndRemoteEventArgs sitecoreArgs))
                return;

            this.ClearCacheForItem(Factory.GetDatabase("Web").GetItem(new ID(sitecoreArgs.RootItemId)), false);
        }

        #endregion

        #region Cache Handling

        /// <summary>
        /// Gets an T from the cache.
        /// </summary>
        public virtual T Get(string key)
        {
            return this.GetObject(key) as T;
        }

        /// <summary>
        /// Gets the cache based on the specified key, or create a new.
        /// </summary>
        public T GetOrCreate(string key)
        {
            var cache = Get(key);
            if (cache == null)
            {
                lock (cacheLock)
                {
                    cache = Get(key);
                    if (cache == null)
                    {
                        cache = Create(key);
                        Set(key, cache);
                    }
                }
            }
            return cache;
        }

        /// <summary>
        /// Creates a new T.
        /// </summary>
        /// <returns></returns>
        public virtual T Create(string key)
        {
            return new T();
        }

        /// <summary>
        /// Set a T in the cache.
        /// </summary>
        public virtual void Set(string key, T value)
        {
            this.SetObject(key, value);
        }

        /// <inheritdoc />
        public virtual void ClearCacheForItem(Item item, bool clearSecondLevelCache)
        {
            if (item == null) return;

            var site = item.GetSite();

            if (site == null)
            {
                if (clearPredicate.ClearOnGlobal)
                    this.Clear(clearSecondLevelCache);
                return;
            }

            this.Remove(site.Name, clearSecondLevelCache);
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public virtual void Clear(bool clearSecondLevelCache)
        {
            Log($"Clearing cache");
            InnerCache.Clear();
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            Clear(true);
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        public virtual void Remove(string key, bool clearSecondLevelCache)
        {
            Log($"Removing {key}");
            this.Remove(key);
        }

        #endregion

        /// <summary>
        /// Disponses the cache.
        /// </summary>
        public void Dispose()
        {
            foreach (var ev in this.Events)
                Event.Unsubscribe(ev.Key, ev.Value);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        protected void Log(string message, bool warn = false)
        {
            var log = $"[{nameof(CustomCache)} {Name}]: {message}";
            if (warn)
                Sitecore.Diagnostics.Log.Warn(log, this);
            else
                Sitecore.Diagnostics.Log.Info(log, this);
        }

        /// <summary>
        /// Logs the specified message and exception.
        /// </summary>
        protected void Log(string message, Exception exception)
        {
            var log = $"[{nameof(CustomCache)} {Name}]: {message}";
                Sitecore.Diagnostics.Log.Error(log, exception, this);
        }
    }
}