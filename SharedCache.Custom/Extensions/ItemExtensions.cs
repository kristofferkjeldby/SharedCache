namespace SharedCache.Custom.Extensions
{
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Web;
    using System.Linq;

    /// <summary>
    /// Item extensions
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Get the site for an item.
        /// </summary>
        public static SiteInfo GetSite(this Item item)
        {
            var sites = Sitecore.Configuration.Factory.GetSiteInfoList();

            if (item?.Paths.IsContentItem ?? false)
                return sites.Where(s => !string.IsNullOrWhiteSpace(s.RootPath) && item.Paths.Path.StartsWithIgnoreCase(s.RootPath)).FirstOrDefault();

            return null;
        }

        /// <summary>
        /// Determines whether an item is derived from a specific template.
        /// </summary>
        public static bool IsDerived(this Item item, ID templateId)
        {
            if (item == null)
                return false;

            return !templateId.IsNull && item.IsDerived(item.Database.Templates[templateId]);
        }

        /// <summary>
        /// Determines whether an item is derived from a specific template.
        /// </summary>
        public static bool IsDerived(this Item item, string templateId)
        {
            if (item == null)
                return false;

            return IsDerived(item, new ID(templateId));
        }

        /// <summary>
        /// Determines whether an item is derived from a specific template.
        /// </summary>
        public static bool IsDerived(this Item item, Item templateItem)
        {
            if (item == null)
                return false;

            if (templateItem == null)
                return false;

            var itemTemplate = TemplateManager.GetTemplate(item);
            return itemTemplate != null && (itemTemplate.ID == templateItem.ID || itemTemplate.DescendsFrom(templateItem.ID));
        }
    }
}