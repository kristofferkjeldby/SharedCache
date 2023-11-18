namespace SharedCache.Core.Serialization
{
    using Sitecore.Data;

    /// <summary>
    /// Serializing an Sitecore ID to the cache
    /// </summary>
    public class IdCacheSerializer : ICacheSerializer<ID>
    {
        /// <inheritdoc/>
        public ID Deserialize(string serializedValue)
        {
            return ID.Parse(serializedValue);
        }

        /// <inheritdoc/>
        public string Serialize(ID obj)
        {
            return obj.ToString();
        }
    }
}