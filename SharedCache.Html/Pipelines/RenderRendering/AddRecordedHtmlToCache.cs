namespace SharedCache.Html.Pipelines.RenderRendering
{
    using SharedCache.Html.Extensions;
    using Sitecore;
    using Sitecore.Mvc.Pipelines.Response.RenderRendering;
    using System;

    /// <summary>
    /// Add recorded html to cache
    /// </summary>
    public class AddRecordedHtmlToCache : Sitecore.Mvc.Pipelines.Response.RenderRendering.AddRecordedHtmlToCache
    {
        /// <inheritdoc />
        protected override void AddHtmlToCache(string cacheKey, string html, RenderRenderingArgs args)
        {
            if (string.IsNullOrEmpty(Context.Site?.SiteInfo?.Name))
                return;

            TimeSpan timeout = this.GetTimeout(args);

            var cache = args.Rendering?.GetHtmlCache(Context.Site?.SiteInfo);

            if (cache != null)
                cache.SetHtml(cacheKey, html, timeout);
        }
    }
}