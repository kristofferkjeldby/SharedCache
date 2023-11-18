namespace SharedCache.Core
{
    /// <summary>
    /// Settings for shared cache framework
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Redis connection string name (the default in Sitecore is redis.sessions)
        /// </summary>
        public static string RedisConnectionStringName => Sitecore.Configuration.Settings.GetSetting(Constants.RedisConnectionStringNameKey, "redis.sessions");

        /// <summary>
        /// File string cache root
        /// </summary>
        public static string FileCacheStringCacheRoot => Sitecore.Configuration.Settings.GetSetting(Constants.FileStringCacheRootSetting);

    }
}