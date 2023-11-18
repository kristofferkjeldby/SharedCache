namespace SharedCache.Core.Serialization
{
    /// <summary>
    /// Serializing a string to the cache
    /// </summary>
    public class StringCacheSerializer : ICacheSerializer<string>
    {
        /// <inheritdoc/>
        public string Deserialize(string serializedValue)
        {
            return serializedValue;
        }

        /// <inheritdoc/>
        public string Serialize(string obj)
        {
            return obj;
        }
    }
}