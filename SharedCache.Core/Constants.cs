namespace SharedCache.Core
{
    /// <summary>
    /// Cache constants
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// The redis connection string key
        /// </summary>
        public const string RedisConnectionStringNameKey = "SharedCache.Core.RedisConnectionStringName";

        /// <summary>
        /// The redis default cache timeout key
        /// </summary>
        public const string RedisDefaultCacheTimeoutKey = "SharedCache.Core.RedisDefaultCacheTimeout";

        /// <summary>
        /// The date time format
        /// </summary>
        public static string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

        /// <summary>
        /// The date time delimiter
        /// </summary>
        public static string DateTimeDelimiter = "|";

        /// <summary>
        /// The file string cache root setting
        /// </summary>
        public static string FileStringCacheRootSetting = "SharedCache.Core.FileStringCacheRoot";
    }
}