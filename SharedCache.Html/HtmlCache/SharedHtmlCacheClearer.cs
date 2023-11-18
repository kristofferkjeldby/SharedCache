namespace SharedCache.Html.HtmlCache
{
    using SharedCache.Html.Extensions;
    using SharedCache.Html.Helpers;
    using SharedCache.Html.Models;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Web;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Html cache clearer
    /// </summary>
    public class SharedHtmlCacheClearer
    {
        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void ClearCache(object sender, EventArgs args)
        {
            ClearCache(args, true);
        }

        /// <summary>
        /// Clears the cache remote.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void ClearCacheRemote(object sender, EventArgs args)
        {
            ClearCache(args, false);
        }

        private void ClearCache(EventArgs args, bool clearSecondLevelCache)
        {
            Assert.ArgumentNotNull(args, "args");

            EventHelper.GetPublishingInfo(args, out Item rootItem, out var clearCacheFlag);

            if (clearCacheFlag == ClearHtmlCacheFlag.None)
                return;

            IEnumerable<SiteInfo> sites = null;

            if (clearCacheFlag.HasFlag(ClearHtmlCacheFlag.IncludeAllSites))
                sites = Sitecore.Configuration.Factory.GetSiteInfoList();
            else
            {
                if (rootItem != null)
                    sites = rootItem.GetSites();
            }
            
            if (sites == null || !sites.Any())
            {
                Log("No sites found for root item");
                return;
            }

            ClearHtmlCaches(sites.ToList(), clearCacheFlag, clearSecondLevelCache);
        }

        /// <summary>
        /// Clears the sites caches.
        /// </summary>>
        public void ClearHtmlCaches(IEnumerable<SiteInfo> sites, ClearHtmlCacheFlag clearCacheFlag, bool clearSecondLevelCache)
        {
            Log($"Clearing html caches for {sites.Count()} sites");

            foreach (var site in sites)
            {
                if (clearCacheFlag.HasFlag(ClearHtmlCacheFlag.ContentHtmlCache))
                    this.ClearHtmlCache(site, Constants.HtmlCacheName, clearSecondLevelCache);

                if (clearCacheFlag.HasFlag(ClearHtmlCacheFlag.StaticHtmlCache))
                    this.ClearHtmlCache(site, Constants.StaticHtmlCacheName, clearSecondLevelCache);
            }

            Log("Done");
        }

        private void ClearHtmlCache(SiteInfo siteInfo, string cacheName, bool clearSecondLevelCache)
        {
            if (siteInfo.GetSharedHtmlCache(cacheName)?.Clear(clearSecondLevelCache) ?? false)
                Log($"{cacheName} cache cleared for {siteInfo.Name}{(clearSecondLevelCache ? " including second level cache" : string.Empty)}");
        }

        private void Log(string message, bool warn = false)
        {
            var log = $"[SharedHtmlCacheClearer]: {message}";
            if (warn)
                Sitecore.Diagnostics.Log.Warn(log, this);
            else
                Sitecore.Diagnostics.Log.Info(log, this);
        }
    }
}