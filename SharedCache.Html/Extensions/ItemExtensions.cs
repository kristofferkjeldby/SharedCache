namespace SharedCache.Html.Extensions
{
    using Sitecore.Data.Items;
    using Sitecore.Web;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Item extensions
    /// </summary>
    public static class ItemExtensions
    {

        /// <summary>
        /// Gets the sites.
        /// </summary>
        /// <param name="item">The item.</param>
        public static IEnumerable<SiteInfo> GetSites(this Item item)
        {
            var sites = Sitecore.Configuration.Factory.GetSiteInfoList();

            if (item?.Paths.IsContentItem ?? false)
                return sites.Where(s => !string.IsNullOrWhiteSpace(s.RootPath) && item.Paths.Path.StartsWithIgnoreCase(s.RootPath));

            return Enumerable.Empty<SiteInfo>();
        }
    }
}