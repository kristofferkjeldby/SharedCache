namespace SharedCache.Core.Serialization
{
    /// <summary>
    /// Interface to implement serialization to the cache
    /// </summary>
    public interface ICacheSerializer<T>
    {
        /// <summary>
        /// Deserializes the specified serialized value.
        /// </summary>
        /// <param name="serializedValue">The serialized value.</param>
        T Deserialize(string serializedValue);

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        string Serialize(T obj);
    }
}