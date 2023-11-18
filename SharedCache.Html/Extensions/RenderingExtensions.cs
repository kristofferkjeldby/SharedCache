namespace SharedCache.Html.Extensions
{
    using SharedCache.Html.HtmlCache;
    using Sitecore.Mvc.Presentation;
    using Sitecore.Web;

    /// <summary>
    /// Rendering extensions
    /// </summary>
    public static class RenderingExtensions
    {
        /// <summary>
        /// Gets the HTML cache.
        /// </summary>
        /// <param name="rendering">The rendering.</param>
        /// <param name="siteInfo">The site information.</param>
        public static SharedHtmlCache GetHtmlCache(this Rendering rendering, SiteInfo siteInfo)
        {
            if (siteInfo == null)
                return null;

            return (rendering?.RenderingItem?.InnerItem?.Fields["UseStaticHtmlCache"]?.Value == "1") ?
            siteInfo.GetSharedHtmlCache(Constants.StaticHtmlCacheName) :
            siteInfo.GetSharedHtmlCache(Constants.ContentHtmlCacheName);
        }
    }
}