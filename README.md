# SharedCache

## Introduction

The SharedCache framework is an extension to the build-in caches in Sitecore XM/XP. It offers the ability for multiple CD instances to share a single _shared cache_ (hosted either in the filesystem, memory or in Redis). 

In a large Sitecore solution with multiple CD servers this will result in improved performance as the cached data from one instance can be reused by other instances. This is especially important if the number of CD instances vary, as new CD instaces will prefetch the cache from the shared cache.

The SharedCache framework offers two kinds of Sitecore caches: 

The _shared html cache_ is a replacement for the build in memory HTML cache offered by Sitecore. The shared html cache still uses memory as a first level cache storage, but will use a shared string cache as a second level cache if a key is not found in the first level cache. 

Also, the SharedCache framework offers three _shared custom caches_ . These caches can be used to store object, list of objects and dictionaries of objects of any serializable type. They can be used as a replacement for the custom caches provided by Sitecore, and offers the possibility to use a second level shared cache as well.

## SharedCache.Core

Fundamental to the functionality of the SharedCache framework is `StringCache` implementations in the `SharedCache.Core` project. A `StringCache` is a simple cache that stores strings, lists of strings of dictionaries of strings. The SharedCache framework offers StringCache implementations using the HTTP session, the HTTP cache, the file system or Redis as a storage mechanism. 

The `StringCache` are not meant to be used directly (although they can), but to be injected into either a shared html cache or one of the shared custom caches as a second level cache. This means that if either the `SharedHtmlCache` or one of the shared custom caches does not directly (in the first level memory cache) has a requested cache key, it will look into the second level cache for the same key. 

So whilethe shared caches still primary stores values in memory, and will only look in e.g., Redis is the local first level cache fails to produce a result. 

### Supported storage mechanisms

The SharedCache comes with a number of string caches using different storage mechanisms:

#### FileStringCache

This string cache used the file system as a storage. It is a rather crude implementation, storing using keys as file names, and is not optimized for production use. This string cache is provided to help local testing or to use a shared cache in a situation where Redis is not available (e.g. on a local developers machine). 

#### IndexedStringCache

To avoid problems with keys containing illegal characters with the `FileStringCache`, the `IndexedStringCache` is offered as an wrapper for the `FileStringCache` or string caches with similiar limitations. It stores keys in an index, circumventing problems with illegal characters. 

#### HttpStringCache

Uses the Http cache as a storage mechanism. Is in-memory only and provided for some of the same reasons as the `FileStringCache` – it allows running simulating a shared cache on a local machine.

#### HttpSessionStringCache

Uses the Http session as storage mechanism. Is in-memory only and provided for some of the same reasons as the `FileStringCache` – it allows running simulating a shared cache on a local machine.

#### RedisStringCache

Offers the same capabilities as the other string caches but uses a Redis database. Will by default use the default Sitecore Redis connection string (`redis.sessions`). This can be changed by using the setting `SharedCache.Core.RedisConnectionStringName`. 

### Isolation

When you create a string cache, you will always provide a name for the string cache. Becuase all the string caches uses a shared storage (e.g. the file system or a Redis database), the `SharedCache.Core.StringCaches.Keys` namespace contains logic to wrap and unwrap keys. This allows two string caches to use the same Redis database, without risking key collisions. It also allows one string cache to be cleared without clearing other caches within the same storage.

### Serialization

To support the storage of an object of any type into the second level string caches, the SharedCache uses JSON serialization. As the SharedHtmlCache already stored strings of HTML, this is relevant only for the shared custom caches. However, the cache mechanism can be overwritten for a specific shared custom cache by implementing the `SharedCache.Core.Serialization.ICacheSerializer<T>` interface:

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

## SharedCache.Html

The shared HTML cache is implemented in the `SharedCache.Html` project. This contains the needed infrastructure to allow allow a single site in Sitecore to use multiple HTML caches which can improve performance on large sites as a publish does not nessasary needs to clear all cached HTML content. In the current implementation each site has two shared HTML caches - one for ordinary content and one for static content (like headers and footers). Which cache to use for a particular rendering is configured by added a shared checkbox to the rendering template called `UseStaticHtmlCache`.

To clear the static HTML cache, the editor must explicit select this during publishing:

![Example](https://raw.githubusercontent.com/kristofferkjeldby/SharedCache/main/readme.png)

For configuration the `SharedCache.Html` addeds the following patch files:

[SharedCache.Html/App_Config/Include/Foundation/SharedCache.Html.CD.config](SharedCache.Html/App_Config/Include/Foundation/SharedCache.Html.CD.config)

```
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/" xmlns:role="http://www.sitecore.net/xmlconfig/role/">
    <sitecore role:require="ContentDelivery">
        <settings>
          <setting name="SharedCache.Html.SecondLevelSharedCustomCacheMethod" value="Redis" />
          <setting name="SharedCache.Html.SharedCustomCacheClearOnly" value="false" />
        </settings>
    </sitecore>
</configuration>
```

And:

[SharedCache.Html/App_Config/Include/Foundation/SharedCache.Html.CM.config](SharedCache.Html/App_Config/Include/Foundation/SharedCache.Html.CM.config)

```
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/" xmlns:role="http://www.sitecore.net/xmlconfig/role/">
    <sitecore role:require="ContentManagement">
        <settings>
          <setting name="SharedCache.Custom.SecondLevelSharedCustomCacheMethod" value="Redis" />
          <setting name="SharedCache.Html.SharedCustomCacheClearOnly" value="true" />
        </settings>
    </sitecore>
</configuration>
```

These files controls the second level cache method used for HTML caching. It also sets the CM server in ClearOnly mode. This means that HTML rendered on the CM server is not added to the second level cache which can prevent HTML content rendered from the master database from being added into the second level cache.

## SharedCache.Custom

These three shared custom caches (`SharedCustomCache`, `SharedCustomListCache` and `SharedCustomDictionaryCache`) are implemented in the `SharedCache.Custom` project and include advanced option to control the clearing of a cache based on the publishing of items.

### Cache clearing

The shared custom caches are added to the Sitecore Cache Manager and can be cleared like any other caches, e.g., via the API or the Cache administrative tool.

However, the shared custom cache also supports cache clearing whenever a `item:saved`, `publish:end` or `publish:end:remote` event is trigged. In a normal scenario only the `publish:end` event would clear the second level cache, whereas all events will clear the local memory cache. The logic to determine whether a specific save or publish event should clear a particular cache is encapsulated in a `ClearPredicate`:

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

Of special notice is the property `ClearOnGlobal` and `UseSiteNameAsCacheKey` properties. The `ClearOnGlobal` determines whether the cache should be cleared for items not belonging to a site (items not descending from `sitecore/content`). 

The `UseSiteNameAsCacheKey` is a bit special. Normally, if this is set for false, the entire cache is cleared is the `DoClear` returns true. However, we often use the custom shared cache in a way so that each site has a key within the cache (e.g. using the `SharedCustomListCache` to contain a list of some objects for each site). 

In that case we do not want to clear the entire cache upon a publish, we simply want to remove the particular key for the published site. If setting the `UseSiteNameAsCacheKey` to true, this is what is going to happen for items that belong to a particular site (global items will still clear the entire cache if `ClearOnGlobal` is true).

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
For configuration the `SharedCache.Custom` addeds the following patch files:

[SharedCache.Custom/App_Config/Include/Foundation/SharedCache.Custom.CD.config](SharedCache.Custom/App_Config/Include/Foundation/SharedCache.Custom.CD.config)

```
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/" xmlns:role="http://www.sitecore.net/xmlconfig/role/">
    <sitecore role:require="ContentDelivery">
        <settings>
            <setting name="SharedCache.Html.SecondLevelHtmlCacheMethod" value="Redis" />
            <setting name="SharedCache.Html.SharedHtmlCacheClearOnly" value="false" />
        </settings>
    </sitecore>
</configuration>
```

And:

[SharedCache.Custom/App_Config/Include/Foundation/SharedCache.Custom.CM.config](SharedCache.Custom/App_Config/Include/Foundation/SharedCache.Custom.CM.config)

```
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/" xmlns:role="http://www.sitecore.net/xmlconfig/role/">
    <sitecore role:require="ContentManagement">
        <settings>
            <setting name="SharedCache.Html.SecondLevelHtmlCacheMethod" value="Redis" />
            <setting name="SharedCache.Html.SharedHtmlCacheClearOnly" value="true" />
        </settings>
    </sitecore>
</configuration>
```

Just like the shared HTML cache setting, these files configures the second level cache method used for the shared custom caches. They also sets the CM server in ClearOnly mode. This means that cache items added on the CM will not be added to the second level cache.

To construct a shared list cache for the object type `MyCacheObject` we use the following constructor:

```
new SharedCustomListCache<MyCacheObject>(
   cacheName,
   new AlwaysClearPredicate(true, true),
   new JsonCacheSerializer<MyCacheObject>(),
);
```

This will create a `SharedCustomListCache` using the second level cache method configured in the `SharedCache.Html.SecondLevelSharedCustomCacheMethod` setting. It will also put the cache on the CM server in clearOnlyMode (configured in the setting `SharedCache.Html.SharedCustomCacheClearOnly`). This means that the cache on the CM server will not add content to the second level cache. 

Both the `StringCache` second level cache and the `clearOnly` exists as optional properties on the constructor, but leaving them out will use the values configured in the config files above. 

In this case the `AlwaysClearPredicate` has been set to `ClearOnGlobal = true` and `UseSiteNameAsCacheKey = true` (the two constructor parameters) which means that if an item is published within a specific site, the whole cache will not be cleared, but the publish will only remove the cache key matching the site name. 

Often instead of using the `AlwaysClearPredicate` clear predicate, a better alternative is to use the `TemplateClearPredicate` which will allow you to configure a list of trigger templates, so only items using these templates will clear the cache upon publish.

Also instead of hard-coding the second level cache, SharedCache offers a StringCacheFactory where the type can be read for e.g. a Sitecore setting. 

## Initialization of shared caches

All the shared caches will do a prefetch upon initialization. This means that they will fetch all keys from the second level cache and load them into the first level memory cache. This is an important component in the optimizations offered by the SharedCache framework for solutions where new CD instances is created - e.g., via auto scaling. This means that all new instances will have filled up caches immediately after initialization.
