namespace SharedCache.Tests.StringCaches
{
    using System.Collections.Generic;
    using Xunit;
    using System;
    using System.Threading;
    using SharedCache.Core.StringCaches;

    public abstract class StringCacheTests
    {
        public abstract StringCache CreateStringCache(string name);

        protected void RunTestStrings()
        {
            var cache = CreateStringCache(nameof(RunTestStrings));
            cache.DisableLogging = true;

            // Clear cache
            cache.Clear();
            Assert.Equal(0, cache.Count());

            // Add two strings to the cache
            cache.SetString("string1", "value1");
            cache.SetString("string2", "value2");

            Assert.Contains("string1", cache.GetStringKeys());
            Assert.Contains("string2", cache.GetStringKeys());
            Assert.Equal(2, cache.GetStringKeys().Count);
            Assert.Equal("value1", cache.GetString("string1"));
            Assert.Equal("value2", cache.GetString("string2"));

            // Empty strings should be added to the cache
            cache.SetString("string4", string.Empty);
            Assert.Equal(string.Empty, cache.GetString("string4"));
            Assert.Equal(3, cache.Count());

            // Remove string from cache
            cache.RemoveString("string2");
            Assert.Null(cache.GetString("string2"));
            Assert.Equal(2, cache.Count());
            Assert.Equal(2, cache.GetStrings().Count);
            Assert.Contains("string1", cache.GetStringKeys());

            // Overwrite existing string in cache
            cache.SetString("string1", "value3");
            Assert.Equal("value3", cache.GetString("string1"));

            // Null keys or values must return false
            Assert.False(cache.SetString(null, "null"));
            Assert.False(cache.SetString("null", null));

            // Clear cache
            cache.Clear();
            Assert.DoesNotContain("string1", cache.GetStringKeys());
            Assert.DoesNotContain("string2", cache.GetStringKeys());
            Assert.Null(cache.GetString("string1"));
            Assert.Null(cache.GetString("string2"));
            Assert.Equal(0, cache.Count());
        }

        protected void RunTestStringsExpire()
        {
            var cache = CreateStringCache(nameof(RunTestStringsExpire));

            // Clear the cache
            cache.Clear();
            Assert.Equal(0, cache.Count());

            // Add two strings with expiry
            cache.SetString("string1", "value1", new TimeSpan(0, 0, 1));
            cache.SetString("string2", "value2", new TimeSpan(0, 30, 0));

            Assert.Contains("string1", cache.GetStringKeys());
            Assert.Contains("string2", cache.GetStringKeys());
            Assert.Equal("value1", cache.GetString("string1"));
            Assert.Equal("value2", cache.GetString("string2"));
            Assert.Equal(2, cache.Count());

            // Wait for string1 to expire
            Thread.Sleep(3000);

            // Check that string1 has been removed from cache
            Assert.DoesNotContain("string1", cache.GetStringKeys());
            Assert.Contains("string2", cache.GetStringKeys());
            Assert.Null(cache.GetString("string1"));
            Assert.Equal("value2", cache.GetString("string2"));
            Assert.Equal(1, cache.Count());

            // Clear the cache
            cache.Clear();

            Assert.DoesNotContain("string1", cache.GetStringKeys());
            Assert.DoesNotContain("string2", cache.GetStringKeys());
            Assert.Null(cache.GetString("string1"));
            Assert.Null(cache.GetString("string2"));
            Assert.Equal(0, cache.Count());
        }

        protected void RunTestList()
        {
            var cache = CreateStringCache(nameof(RunTestList));
            cache.DisableLogging = true;

            var list1 = new List<string>()
            {
                { "value1" },
                { "value2" },
            };

            var list2 = new List<string>()
            {
                { "value3" },
                { "value4" }
            };

            // Clear the cache
            cache.Clear();
            Assert.Equal(0, cache.Count());

            // Add a single list to the cache
            cache.SetList("list1", list1);
            Assert.Equal(1, cache.Count());
            Assert.Single(cache.GetLists());
            Assert.Contains("list1", cache.GetListKeys());

            // Add list2 to the cache
            cache.SetList("list2", list2);
            Assert.Equal(2, cache.Count());
            Assert.Equal(2, cache.GetLists().Count);
            Assert.Equal(2, cache.GetListKeys().Count);
            Assert.Contains("list1", cache.GetListKeys());
            Assert.Contains("list2", cache.GetListKeys());

            var lists = cache.GetLists();
            Assert.Equal(2, lists.Count);
            Assert.Equal("value2", lists["list1"][1]);
            Assert.Equal("value3", lists["list2"][0]);

            // Remove list 2 - should return true as list exist
            Assert.True(cache.RemoveList("list2"));

            // Remove list 2 again - should return false
            Assert.False(cache.RemoveList("list2"));

            // There should be one list left - list 1
            Assert.Equal(1, cache.Count());

            // Get values from lists
            Assert.Equal("value1", cache.GetList("list1")[0]);

            // Add empty list
            cache.SetList("emptyList", new List<string>());
            Assert.Contains("emptyList", cache.GetListKeys());

            // Nulls or lists with null values must return false
            Assert.False(cache.SetList(null, new List<string>()));
            Assert.False(cache.SetList("null", null ));
            Assert.False(cache.SetList("null", new List<string> { null }));

            // Clear the cache
            cache.Clear();
            Assert.Equal(0, cache.Count());
        }

        protected void RunTestDictionary()
        {
            var cache = CreateStringCache(nameof(RunTestDictionary));
            cache.DisableLogging = true;

            // Clear the cache
            cache.Clear();
            Assert.Equal(0, cache.Count());

            var dictionary1 = new Dictionary<string, string>()
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "emptyKey", string.Empty }, // Not a good idea, but allowed
                { string.Empty, "emptyValue" }, // Not a good idea, but allowed
            };

            var dictionary2 = new Dictionary<string, string>()
            {
                { "key3", "value3" },
                { "key4", "value4" }
            };

            // Add dictionary1 to cache
            cache.SetDictionary("dictionary1", dictionary1);
            Assert.Single(cache.GetDictionaries());
            Assert.Equal(1, cache.Count());

            // Assert that only the correct elements have been added
            var dictionary1FromCache = cache.GetDictionary("dictionary1");
            Assert.Equal("value1", dictionary1FromCache["key1"]);
            Assert.Equal("value2", dictionary1FromCache["key2"]);
            Assert.Equal(string.Empty, dictionary1FromCache["emptyKey"]);
            Assert.Equal("emptyValue", dictionary1FromCache[string.Empty]);
            Assert.Contains(string.Empty, dictionary1FromCache.Keys);
            Assert.Equal(4, dictionary1FromCache.Keys.Count);
            var keys = cache.GetDictionaryKeys();
            Assert.Contains("dictionary1", keys);

            // Add dictionary 2 to the cache
            cache.SetDictionary("dictionary2", dictionary2);
            Assert.Equal(2, cache.Count());

            var dictionaries = cache.GetDictionaries();

            Assert.Equal(2, dictionaries.Count);
            Assert.Equal("value1", dictionaries["dictionary1"]["key1"]);
            Assert.Equal("value4", dictionaries["dictionary2"]["key4"]);

            // Remove dictionary 2 from cache
            Assert.True(cache.RemoveDictionary("dictionary2"));
            Assert.Equal(1, cache.Count());

            // Add and update keys
            Assert.True(cache.SetDictionaryElement("dictionary1", "key3", "value3"));
            Assert.True(cache.SetDictionaryElement("dictionary1", "key3", "value4"));
            Assert.Equal("value4", cache.GetDictionary("dictionary1")["key3"]);
            Assert.True(cache.SetDictionaryElement("dictionary1", string.Empty, "emptyValue2"));
            Assert.True(cache.SetDictionaryElement("dictionary1", "emptyKey2", string.Empty));
            Assert.Equal(string.Empty, cache.GetDictionary("dictionary1")["emptyKey2"]);
            Assert.Equal("emptyValue2", cache.GetDictionaryElement("dictionary1", string.Empty));
            Assert.Null(cache.GetDictionaryElement("dictionary1", "nonExistingKey"));

            // Neither keys nor values must be null
            Assert.False(cache.SetDictionary(null, new Dictionary<string, string> { { "null", "null" } }));
            Assert.False(cache.SetDictionary("null", new Dictionary<string, string> { { "null", null } }));
            Assert.False(cache.SetDictionary("null", null));
            Assert.False(cache.SetDictionaryElement(null, "null", "null"));
            Assert.False(cache.SetDictionaryElement("null", null, "null"));
            Assert.False(cache.SetDictionaryElement("null", "null", null));

            // Manipulate nonexisting dictionary
            Assert.False(cache.RemoveDictionary("dictionary2"));
            Assert.False(cache.SetDictionaryElement("dictionary2", "key5", "value5"));
            Assert.False(cache.RemoveDictionaryElement("dictionary2", "key5"));

            // Add empty dictionary
            cache.SetDictionary("emptyDictionary", new Dictionary<string, string>());
            Assert.Contains("emptyDictionary", cache.GetDictionaryKeys());

            // Clear the cache
            cache.Clear();
            Assert.Equal(0, cache.Count());
        }

        protected void RunTestMixed()
        {
            var cache = CreateStringCache(nameof(RunTestMixed));
            cache.DisableLogging = true;

            cache.Clear();

            Assert.Equal(0, cache.Count());

            var dictionary1 = new Dictionary<string, string>()
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            var dictionary2 = new Dictionary<string, string>()
            {
                { "key3", "value3" },
                { "key4", "value4" }
            };

            var generic = new Dictionary<string, string>()
            {
                { "key5", "value5" },
                { "key6", "value6" }
            };

            // Add four dictionaries
            cache.SetDictionary("dictionary1", dictionary1);
            cache.SetDictionary("dictionary1", dictionary1); // Should have no effect
            cache.SetDictionary("dictionary2", dictionary2);
            cache.SetDictionary("generic", generic);

            Assert.Equal(3, cache.Count());
            Assert.Equal(3, cache.GetDictionaryKeys().Count);

            var dictionaries = cache.GetDictionaries();

            Assert.Equal(3, dictionaries.Count);
            Assert.Equal("value1", dictionaries["dictionary1"]["key1"]);
            Assert.Equal("value4", dictionaries["dictionary2"]["key4"]);

            // Add 4 strings - generic should not be overwritten
            cache.SetString("string1", "value1");
            cache.SetString("string1", "value1"); // Should have no effect
            cache.SetString("string2", "value2"); 
            cache.SetString("string3", "value3");
            cache.SetString("emptyString", string.Empty);
            cache.SetString("generic", "value5"); // Should not overwrite existing dictionary

            Assert.Equal(3, cache.GetDictionaryKeys().Count);
            Assert.Equal(3, cache.GetDictionaries().Count);
            Assert.Equal(5, cache.GetStringKeys().Count);
            Assert.Equal(5, cache.GetStrings().Count);
            Assert.Equal(8, cache.Count());

            var list1 = new List<string>()
            {
                { "value1" },
                { "value2" }
            };

            var list2 = new List<string>()
            {
                { "value3" },
                { "value4" }
            };

            cache.SetList("list1", list1);
            cache.SetList("list2", list2);

            Assert.Equal(3, cache.GetDictionaryKeys().Count);
            Assert.Equal(3, cache.GetDictionaries().Count);
            Assert.Equal(5, cache.GetStringKeys().Count);
            Assert.Equal(5, cache.GetStrings().Count);
            Assert.Equal(2, cache.GetListKeys().Count);
            Assert.Equal(2, cache.GetLists().Count);

            Assert.Equal(10, cache.Count());

            cache.Clear();

            Assert.Equal(0, cache.Count());
        }
    }
}
