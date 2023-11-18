namespace SharedCache.Html
{
    using Sitecore;
 
    /// <summary>
    /// Constants
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// The html cache size
        /// </summary>
        public static long HtmlCacheSize = StringUtil.ParseSizeString("25MB");

        /// <summary>
        /// The content html cache name
        /// </summary>
        public static string ContentHtmlCacheName = "html";

        /// <summary>
        /// The static html cache name
        /// </summary>
        public static string StaticHtmlCacheName = "statichtml";

        /// <summary>
        /// The second level html cache method setting
        /// </summary>
        public static string SecondLevelHtmlCacheMethodSetting = "SharedCache.Html.SecondLevelHtmlCacheMethod";

        /// <summary>
        /// The shared html cache clear only setting
        /// </summary>
        public static string SharedHtmlCacheClearOnlySetting = "SharedCache.Html.SharedHtmlCacheClearOnly";
    }
}