# SharedCache



## Introduction

The SharedCache framework is an extension to the build in caches in SitecoreXM/XP. It offers the ability for multiple CD instances to share a single _shared cache_ (hosted either in the filesystem, memory or in Redis). In a large Sitecore solution with multiple CD servers this will result in improved performance as the cached data from one instance can be reused by other instances. This is especially important if the number of CD instances vary, as new CD instaces will prefetch the cache from the shared cache.

The SharedCache framework offers two kinds of Sitecore caches: 

The _shared html cache_ is a replacement for the build in memory HTML cache offered by Sitecore. The shared html cache still primarily uses memory as a cache storage, but will use a shared string cache as a second level cache. 

Also, the SharedCache framework offers three _shared custom caches_ . These caches can be used to store object, list of objects and dictionaries of objects of any serializable type. They can be used as a replacement for the custom caches provided by Sitecore, and offers the possiblity to use a second level shared cache.

## SharedCache.Core

Fundamental to the functionality of the SharedCache framework is `StringCache` implementions in the `SharedCache.Core` project. A `StringCache` is a simple cache that stores strings, lists of strings of dictionaries of strings. The SharedCache framework offers StringCache implementations using the HTTP session, the HTTP cache, the file system or Redis as a storage mechanism. 

The `StringCache` are not meant to be used directly (although they can), but to be injected into either a shared html cache or one of the shared custom caches as a second level cache. This means that if either the `SharedHtmlCache` or one of the shared custom caches does not directly (in memory) has a requested cache key, it will look into the second level cache for the same key. 

This means that the shared caches still primary stores values in memory, and will only look in Redis is the local memory cache fails to produce a result. 

### Supported storage mechanisms

The SharedCache comes with a number of string caches using different storage mechanisms:

#### FileStringCache

This string cache used the file system as a storage. It is a rather crude implementation, storing using keys as file names, and is not optimized for production use. These string cache is provided to help local testing or to use a shared cache in a situation where Redis is not available (e.g. on a local developers machine). 

#### IndexedStringCache

To avoid problems with keys containing illegal characters with the `FileStringCache`, the `IndexedStringCache` is offered as an wrapper for the `FileStringCache` or string caches with similiar limitations. It stores keys in a index, circumventing problems with illegal characters. 

#### HttpStringCache

Uses the Http cache as a storage mechanism. Is in-memory only, and provided for some of the same reasons as the `FileStringCache` – it allows running simulating a shared cache on a local machine.

#### HttpSessionStringCache

Uses the Http session as storage mechanism. Is in-memory only, and provided for some of the same reasons as the `FileStringCache` – it allows running simulating a shared cache on a local machine.

#### RedisStringCache

Offers the same capabilities as the other string caches, but uses a Redis database. Will by default use the default Sitecore Redis connection string (`redis.sessions`).

### Isolation

When you create a string cache, you will always provide a name for the string cache. Becuase all the string caches uses a shared storage (e.g. the file system or a Redis database), the `SharedCache.Core.StringCaches.Keys` namespace contains logic to wrap and unwrap keys. This allow two string caches to use the same Redis database, without risking key collisions. It also allow one string cache to be cleared without clearing other caches within the same storage.

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

## Shared HTML cache

The shared HTML cache is implemented in the `SharedCache.Html` project. This contains the needed infrastructure to allow allow a single site in Sitecore to use multiple HTML caches which can improve performance on large sites as a publish does not nessasary needs to clear all cached HTML content. In the current implementation each site has two shared HTML caches - one for ordinary content and one for static content (like headers and footers). Which cache to use for a particular rendering is configured by added a shared checkbox to the rendering template called `UseStaticHtmlCache`.

To clear the static HTML cache, the editor must explicit select this during publishing:

![Example](https://raw.githubusercontent.com/kristofferkjeldby/SharedCache/main/readme.png)

## Shared custom caches

These three shared custom caches (`SharedCustomCache`, `SharedCustomListCache` and `SharedCustomDictionaryCache`) are implemented in the `SharedCache.Custom` project, and include advanced option to control the clearing of a cache based on the publishing of items.

### Cache clearing

The shared custom cache supports cache clearing whenever a `item:saved`, `publish:end` or `publish:end:remote` event is trigged. In a normal scenario only the `publish:end` event would clear the second level cache, whereas all events will clear the local memory cache. The logic to determine whether a specific save or publish event should clear a particular cache is encapsulated in a `ClearPredicate`:

```
namespace SharedCache.Custom.ClearPredicates
{
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Events;
    using Sitecore.Data.Items;
    using Sitecore.Events;

    /// <summary>
    /// Abstract class for a clear predicate
    /// </summary>
    public abstract class ClearPredicate
    {
        /// <summary>
        /// Gets or sets a value indicating whether to clear on global publish.
        /// </summary>
        public abstract bool ClearOnGlobal { get; }

        /// <summary>
        /// Gets a value indicating whether use site name as cache key
        /// </summary>
        public abstract bool UseSiteNameAsCacheKey { get; } 

        /// <summary>
        /// Determines whether to clear the cache.
        /// </summary>
        public abstract bool DoClear(Item item);
    }
}
```

Of special notice is the property `ClearOnGlobal` and `UseSiteNameAsCacheKey`. The `ClearOnGlobal` determines whether the cache should be cleared for items not belonging to a site (items not from `Sitecore/Content`). The `UseSiteNameAsCacheKey` is a bit special. Normally, if this is set for false, the entire cache is cleared is the `DoClear` returns true. However, I often use the custom shared cache in a way so that each site has a key within the cache (e.g. using the `SharedCustomListCache` to contain a list of some objects for each site). In that case I do not want to clear the entire cache upon a publish, I simply want to remove the particular key for the published site. If setting the `UseSiteNameAsCacheKey` to true, this is what is going to happen for items that belong to a particular site (global items will still clear the entire cache if ClearOnGlobal is true).

The logic for the clearing of shared custom caches can be described using this pseudo code:

```
if DoClear

  if IsGlobalItem <-- Item not part of a site (e.g. a template)
    if ClearOnGlobal 
      clear
      if publish:end
         clear string cache

  if IsSiteItem <-- Item not part of a site (e.g. a sitecore/content/Home)
    if UseSiteNameAsCacheKey 
      remove key
      if publish:end
         remove key from string cache
    else
      clear
      if publish:end
         clear string cache
```










