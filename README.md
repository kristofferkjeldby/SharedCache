# SharedCache

Redis cache framework for Sitecore XM/XP

![Example](https://raw.githubusercontent.com/kristofferkjeldby/SharedCache/main/readme.png)

## Introduction

The SharedCache framework is an extension to the build in caches in Sitecore. It offers the ability for multiple CD instances to share a single "shared cache" (hosted either in the filesystem or in Redis). In a large Sitecore solution with multiple CD servers this will result in improved performance as the cached data from one instance can be reused by other instances. This is especially important if the number of CD instances vary, as new CD instaces will prefetch the cache from the shared cache.

The SharedCache framework offers two kinds of Sitecore caches: 

The `SharedHtmlCache` is a replacement for the build in memory HTML cache offered by Sitecore. The `SharedHtmlCache` still primarily uses memory as a cache storage, but will use a shared cache as a second level cache. This has been implemented in the `SharedCache.Html` project. This project also allow a single site in Sitecore to use multiple HTML caches which can improve performance on large sites as a publish does not nessasary needs to clear all cached HTML content.

Also, the SharedCache framework offers three _shared custom caches_ (`SharedCustomCache`, `SharedCustomListCache` and `SharedCustomDictionaryCache`). These caches can be used to store object, list of objects and dictionaries of objects of any serializable type. They can be used as a replacement for the custom caches provided by Sitecore, and offers the possiblity to use a second level shared cache. These cache are implemented in the `SharedCache.Custom` project, and include advanced option to control the clearing of a cache based on the publishing of items.

## String caches

Fundamental to the functionality of the SharedCache framework is `StringCache` implementions in the `SharedCache.Core` project. A `StringCache` is a simple cache that stores strings, lists of strings of dictionaries of strings. The SharedCache framework offers StringCache implementations using the HTTP session, the HTTP cache, the file system or Redis as a storage mechanism. The `StringCache` are not meant to be used directly, but to be injected into either a `SharedHtmlCache` or one of the _shared custom caches_ as a second level cache. This means that if either the `SharedHtmlCache` or one of the shared custom caches does not directly (in memory) has a requested cache key, it will look into the second level cache for the same key. This means that the shared caches still primary stores values in memory, and will only look in Redis is the local memory cache fails to produce a result. 

## Shared custom caches

### Serialization

To support the storage of a object of any type into the second level string caches, the SharedCache uses JSON serialization. As the SharedHtmlCache already stored strings of HTML, this is relevant only for the shared custom caches. However, the cache mechanism can be overwritten for a specific shared custom cache by implementing the `SharedCache.Core.Serialization.ICacheSerializer<T>` interface:

```
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
```





