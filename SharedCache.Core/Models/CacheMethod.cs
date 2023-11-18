namespace SharedCache.Core.Models
{
    /// <summary>
    /// Cache method enum
    /// </summary>
    public enum CacheMethod
    {
        /// <summary>
        /// The file system
        /// </summary>
        File,

        /// <summary>
        /// The Redis database
        /// </summary>
        Redis,

        /// <summary>
        /// The memory (http cache)
        /// </summary>
        Memory
    }
}