namespace SharedCache.Html.Models
{
    using System;

    /// <summary>
    /// Clear html cache flags
    /// </summary>
    [Flags]
    public enum ClearHtmlCacheFlag
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// The content HTML
        /// </summary>
        ContentHtmlCache = 1,

        /// <summary>
        /// The static HTML
        /// </summary>
        StaticHtmlCache = 2,

        /// <summary>
        /// The both
        /// </summary>
        Both = 3,

        /// <summary>
        /// Include all sites
        /// </summary>
        IncludeAllSites = 4
    }
}