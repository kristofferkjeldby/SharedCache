namespace SharedCache.Html.HtmlCache
{
    using SharedCache.Core.Models;
    using SharedCache.Core.Providers;
    using Sitecore.Caching;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Diagnostics.PerformanceCounters;
    using System;

    /// <summary>
    /// An implementation of a shared HTML cache using a string cache as a second level cache.
    /// </summary>
    public class SharedHtmlCache : GenericCache
    {
        private readonly StringCache secondLevelCache;
        private readonly bool clearOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedHtmlCache"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="maxSize">The maximum size.</param>
        /// <param name="secondLevelCache">The second level cache.</param>
        /// <param name="clearOnly">if set to <c>true</c> [clear only].</param>
        public SharedHtmlCache(string name, long maxSize, StringCache secondLevelCache, bool clearOnly) : base((CacheManager.FindCacheByName<string>(name) as GenericCache)?.InnerCache ?? CacheManager.GetNamedInstance(name, maxSize, false))
        {
            this.secondLevelCache = secondLevelCache;
            this.clearOnly = clearOnly;

            CacheManager.Register(this);
            
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
                    this.SetObject(kv.Key, kv.Value);

                Log($"Initialized {Name} with {kvs.Count} items", false);
            }
            catch (Exception ex)
            {
                Log($"Initialized {Name} failed", ex);
            }
        }

        /// <summary>
        /// Gets the HTML.
        /// </summary>
        public string GetHtml(string key)
        {
            Assert.ArgumentNotNull(key, nameof(key));

            // Read html from memory cache
            string html = GetString(key);
            if (html != null)
            {
                Log($"Html read from memory ({key})");
                CachingCount.HtmlCacheHits.Increment(1L);
                return html;
            }

            if (secondLevelCache == null || clearOnly)
            {
                CachingCount.HtmlCacheMisses.Increment(1L);
                return html;
            }

            // Read html from shared html cache
            try
            {
                html = secondLevelCache.GetString(key);
            }
            catch (Exception ex)
            {
                Log("Could not read html from second level cache", ex);
            }

            if (html != null)
            {
                Log($"Html read from second level cache ({key})");

                // Add cache value to the memory cache
                this.SetString(key, html);
                CachingCount.HtmlCacheHits.Increment(1L);
                return html;
            }

            CachingCount.HtmlCacheMisses.Increment(1L);
            return html;
        }


        private void Set(string key, string html, TimeSpan expiry)
        {
            this.SetString(key, html, DateTime.UtcNow + expiry);

            if (secondLevelCache == null || clearOnly)
            {
                return;
            }

            // Write html to shared html cache
            try
            {
                secondLevelCache.SetString(key, html, expiry);
            }
            catch (Exception ex)
            {
                Log("Could not write html from second level cache", ex);
            }
        }

        /// <summary>
        /// Sets the HTML
        /// </summary>
        public void SetHtml(string key, string html, TimeSpan timeout)
        {
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(html, nameof(html));

            timeout = timeout == TimeSpan.Zero ? Settings.Caching.HtmlLifetime == TimeSpan.Zero ? new TimeSpan(24, 0, 0) : Settings.Caching.HtmlLifetime : timeout;

            this.Set(key, html, timeout);
        }

        /// <summary>
        /// Clears this cache
        /// </summary>
        public bool Clear(bool clearSecondLevelCache)
        {
            InnerCache.Clear();

            if (!clearSecondLevelCache)
                return true;

            // Clear second level cache
            try
            {
                this.secondLevelCache?.Clear();
            }
            catch (Exception ex)
            {
                Log("Could not clear second level cache", ex);
            }

            return true;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            Clear(false);
        }

        private void Log(string message, bool warn = false)
        {
            var log = $"[SharedHtmlCache {Name}]: {message}";
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
            var log = $"[SharedHtmlCache {Name}]: {message}";
            Sitecore.Diagnostics.Log.Error(log, exception, this);
        }
    }
}