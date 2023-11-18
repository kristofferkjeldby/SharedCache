namespace SharedCache.Core.Helpers
{
    using SharedCache.Core.Models;
    using SharedCache.Core.StringCaches;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;

    /// <summary>
    /// Factory for creating string caches
    /// </summary>
    public class StringCacheFactory
    {
        private static Dictionary<string, StringCache> stringCaches = new Dictionary<string, StringCache>();
        private static object _lock = new object();

        /// <summary>
        /// Gets the or create string cache.
        /// </summary>
        /// <param name="cacheName">Name of the cache.</param>
        /// <param name="cacheMethod">The cache method.</param>
        public static StringCache GetOrCreateStringCache(string cacheName, CacheMethod cacheMethod)
        {
            if (stringCaches.ContainsKey(cacheName))
                return stringCaches[cacheName];

            lock(_lock)
            {
                if (stringCaches.ContainsKey(cacheName))
                    return stringCaches[cacheName];

                StringCache stringCache;
                
                switch (cacheMethod)
                {
                    case CacheMethod.File:
                        stringCache = new IndexedStringCache(new FileStringCache(cacheName, Path.Combine(Settings.FileCacheStringCacheRoot, cacheName)));
                        break;
                    case CacheMethod.Redis:
                        stringCache = new RedisStringCache(cacheName);
                        break;
                    case CacheMethod.Memory:
                        stringCache = new HttpStringCache(cacheName, HttpRuntime.Cache);
                        break;
                    default:
                        return null;
                }

                stringCaches.Add(cacheName, stringCache);

                return stringCache;
            }
        }

        /// <summary>
        /// Gets the or create string cache.
        /// </summary>
        /// <param name="cacheName">Name of the cache.</param>
        /// <param name="cacheMethodString">The cache method string.</param>
        public static StringCache GetOrCreateStringCache(string cacheName, string cacheMethodString)
        {
            var cacheMethod = GetCacheMethod(cacheMethodString);
            if (!cacheMethod.HasValue)
                return null;
            return GetOrCreateStringCache(cacheName, cacheMethod.Value);
        }

        private static CacheMethod? GetCacheMethod(string cacheMethodString)
        {
            if (Enum.TryParse<CacheMethod>(cacheMethodString, out var cacheMethod))
                return cacheMethod;
            return null;
        }
    }
}