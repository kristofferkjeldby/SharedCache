namespace SharedCache.Html.Extensions
{
    using SharedCache.Core.Helpers;
    using SharedCache.Html;
    using SharedCache.Html.HtmlCache;
    using Sitecore.Web;
    using System.Collections.Generic;

    /// <summary>
    /// Site info extensions
    /// </summary>
    public static class SiteInfoExtensions
    {
        static readonly Dictionary<string, SharedHtmlCache> htmlCaches = new Dictionary<string, SharedHtmlCache>();
        static readonly object _lock = new object();

        /// <summary>
        /// Gets the HTML cache for a given site and cache name.
        /// </summary>
        public static SharedHtmlCache GetSharedHtmlCache(this SiteInfo siteInfo, string cacheName)
        {
            if (string.IsNullOrEmpty(siteInfo?.Name))
                return null;

            var key = string.Join("_", siteInfo.Name.Replace("-", "_"), cacheName);

            var cacheMethod = Sitecore.Configuration.Settings.GetSetting(Constants.SecondLevelHtmlCacheMethodSetting, "Memory");
            var clearOnly = Sitecore.Configuration.Settings.GetBoolSetting(Constants.SharedHtmlCacheClearOnlySetting, false);
            
            if (htmlCaches.ContainsKey(key))
            {
                return htmlCaches[key];
            }
            else
            {
                lock(_lock)
                {
                    if (!htmlCaches.ContainsKey(key))
                        htmlCaches.Add(
                            key,
                            new SharedHtmlCache(key, Constants.HtmlCacheSize, StringCacheFactory.GetOrCreateStringCache(key, cacheMethod), clearOnly)
                        );
                }
            }

            return htmlCaches[key];
        }
    }
}