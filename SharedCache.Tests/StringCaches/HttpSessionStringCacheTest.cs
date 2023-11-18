namespace SharedCache.Tests.StringCaches
{
    using SharedCache.Core.StringCaches;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web;
    using Xunit;

    public class HttpSessionStringCacheTests : StringCacheTests
    {
        [Fact]
        public void TestStrings()
        {
            RunTestStrings();
        }

        [Fact]
        public void TestStringsExpire()
        {
            RunTestStringsExpire();
        }

        [Fact]
        public void TestList()
        {
            RunTestList();
        }

        [Fact]
        public void TestDictionary()
        {
            RunTestDictionary();
        }

        [Fact]
        public void TestMixed()
        {
            RunTestMixed();
        }

        public override StringCache CreateStringCache(string name)
        {
            return new HttpSessionStringCache(name, new MockHttpSession());
        }

        public class MockHttpSession : HttpSessionStateBase
        {
            private readonly NameValueCollection keyCollection = new NameValueCollection();
            Dictionary<string, object> sessionStorage = new Dictionary<string, object>();

            public override object this[string key]
            {
                get {
                    if (!sessionStorage.ContainsKey(key))
                        return null;
                    return sessionStorage[key]; 
                }
                set { 
                    sessionStorage[key] = value;
                    keyCollection[key] = null;
                }
            }

            public override void Add(string key, object value)
            {
                sessionStorage[key] = value;
                keyCollection[key] = null;
            }

            public override void Remove(string key)
            {
                sessionStorage.Remove(key);
                keyCollection.Remove(key);
            }

            public override void Clear()
            {
                sessionStorage.Clear();
                keyCollection.Clear();
            }

            public override IEnumerator GetEnumerator()
            {
                return sessionStorage.GetEnumerator();
            }

            public override NameObjectCollectionBase.KeysCollection Keys
            {
                get { return keyCollection.Keys; }
            }
        }
    }
}
