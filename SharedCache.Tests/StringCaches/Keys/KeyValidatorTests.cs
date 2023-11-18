namespace SharedCache.Tests.StringCaches.Keys
{
    using SharedCache.Core.Providers.Keys;
    using System;
    using Xunit;

    /// <summary>
    /// Key validator tests
    /// </summary>
    public class KeyValidatorTests
    {
        [Fact]
        public void TestKeyValidator()
        {
            var keyValidator = new KeyValidator(KeyValidationMode.CheckAndFail, KeyValidationMode.CheckAndFail, new[] { '-' });

            Assert.Throws<Exception>(() =>
                {
                    // This should throw an exception when we try to validate a illegal char
                    keyValidator.ValidateKey("ABC-DEF");
                }
            );

            Assert.Equal("ABCDEF", keyValidator.ValidateKey("ABCDEF"));
        }

    }
}
