namespace SharedCache.Core.Serialization
{
    using Newtonsoft.Json;

    /// <summary>
    /// Serializing an object to the cache using JSON
    /// </summary>
    public class JsonCacheSerializer<T> : ICacheSerializer<T>
    {
        /// <inheritdoc/>
        public T Deserialize(string serializedValue)
        {
            return JsonConvert.DeserializeObject<T>(serializedValue);
        }

        /// <inheritdoc/>
        public string Serialize(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}