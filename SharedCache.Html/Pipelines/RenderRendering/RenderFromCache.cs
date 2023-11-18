namespace SharedCache.Html.Pipelines.RenderRendering
{
    using SharedCache.Html.Extensions;
    using Sitecore;
    using Sitecore.Abstractions;
    using Sitecore.Mvc.Pipelines.Response.RenderRendering;
    using Sitecore.Mvc.Presentation;
    using System.IO;

    /// <summary>
    /// Get rendering from cache
    /// </summary>
    public class RenderFromCache : Sitecore.Mvc.Pipelines.Response.RenderRendering.RenderFromCache
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderFromCache"/> class.
        /// </summary>
        /// <param name="rendererCache">The renderer cache.</param>
        /// <param name="baseClient">The base client.</param>
        public RenderFromCache(RendererCache rendererCache, BaseClient baseClient) : base(rendererCache, baseClient)
        {
        }

        /// <inheritdoc />
        protected override bool Render(string cacheKey, TextWriter writer, RenderRenderingArgs args)
        {
            if (string.IsNullOrEmpty(Context.Site?.SiteInfo?.Name))
                return false;

            var cache = args.Rendering?.GetHtmlCache(Context.Site?.SiteInfo);

            if (cache == null)
                return false;

            var html = cache.GetHtml(cacheKey);

            if (string.IsNullOrEmpty(html))
                return false;

            writer.Write(html);

            return true;
        }
    }
}