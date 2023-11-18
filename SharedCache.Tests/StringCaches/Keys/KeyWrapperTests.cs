namespace SharedCache.Tests.StringCaches.Keys
{
    using SharedCache.Core.StringCaches.Keys;
    using Xunit;

    /// <summary>
    /// Key wrapper tests
    /// </summary>
    public class KeyWrapperTests
    {
        [Fact]
        public void TestKeyWrapperTests()
        {
            var keyValidator = new KeyValidator(KeyValidationMode.CheckAndFail, KeyValidationMode.CheckAndFail, new [] {'_', '*'});

            var keyWrapper = new KeyWrapper("test", '*', ".txt", '_', keyValidator);

            Assert.Equal("ABC", keyWrapper.UnwrapKey(keyWrapper.WrapKey("ABC", KeyType.str), KeyType.str));
            Assert.Equal(2, keyWrapper.UnwrapListElementKey(keyWrapper.WrapListElementKey("ABC", 2)));
            Assert.Equal("DEF", keyWrapper.UnwrapDictionarytElementKey(keyWrapper.WrapDictionaryElementKey("ABC", "DEF")));
        }

    }
}
