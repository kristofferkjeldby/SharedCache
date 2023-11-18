namespace SharedCache.Core.StringCaches.Keys
{
    /// <summary>
    /// Key types - Main keys must me 3 chars long
    /// </summary>
    public enum KeyType
    {
        /// <summary>
        /// String key type
        /// </summary>
        str,
        
        /// <summary>
        /// List key type
        /// </summary>
        lst,

        /// <summary>
        /// Lists element key
        /// </summary>
        lstx,

        /// <summary>
        /// Dictionary main key type
        /// </summary>
        dic,

        /// <summary>
        /// Dictionary element key
        /// </summary>
        dicx
    }
}