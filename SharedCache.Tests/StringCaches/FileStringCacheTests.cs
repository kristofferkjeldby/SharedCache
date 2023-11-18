namespace SharedCache.Tests.StringCaches
{
    using SharedCache.Core.StringCaches;
    using Xunit;

    public class FileStringCacheTests : StringCacheTests
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
            return new FileStringCache(name, ".");
        }
    }
}
