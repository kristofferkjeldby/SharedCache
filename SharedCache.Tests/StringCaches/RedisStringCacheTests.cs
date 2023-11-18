namespace SharedCache.Tests.StringCaches
{
    using SharedCache.Core.StringCaches;
    using System.Configuration;
    using Xunit;

    public class RedisStringCacheTests : StringCacheTests
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
            return new RedisStringCache(string.Concat(nameof(RedisStringCacheTests), name), ConfigurationManager.AppSettings["redis.sessions"]);
        }
    }
}
