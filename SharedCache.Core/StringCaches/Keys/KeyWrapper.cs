namespace SharedCache.Core.Providers.Keys
{
    using System;
    using System.Linq;

    /// <summary>
    /// Helper class for wrapping keys
    /// </summary>
    public class KeyWrapper
    {
        private readonly string @namespace;
        private readonly char? wildcard;
        private readonly string postfix;
        readonly char divider;
        private readonly KeyValidator keyValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyWrapper"/> class.
        /// </summary>
        /// <param name="namespace">The namespace.</param>
        /// <param name="wildcard">The wildcard.</param>
        /// <param name="postfix">The postfix.</param>
        /// <param name="divider">The divider.</param>
        /// <param name="keyValidator">The key validator.</param>
        public KeyWrapper(string @namespace, char? wildcard, string postfix, char divider, KeyValidator keyValidator)
        {
            if (keyValidator.ContainsIllegalChars(@namespace))
                throw new Exception($"Namespace {@namespace} contains one or more illegal chars");

            if (keyValidator.ContainsIllegalChars(postfix))
                throw new Exception($"Postfix {postfix} contains one or more illegal chars");

            if (!keyValidator.IsIllegalChar(divider))
                throw new Exception($"Divider {divider} must be an illegal char");

            if (wildcard.HasValue && !keyValidator.IsIllegalChar(wildcard.Value))
                throw new Exception($"Wildcard {wildcard} must be an illegal char");

            this.keyValidator = keyValidator;
            this.wildcard = wildcard;
            this.postfix = postfix;
            this.divider = divider;
            this.@namespace = @namespace;
        }

        /// <summary>
        /// Gets the wrap pattern for the entire cache.
        /// </summary>
        public string WrapPattern()
        {
            return CreatePattern(string.Empty);
        }

        #region Keys

        /// <summary>
        /// Wraps a key.
        /// </summary>
        public string WrapKey(string key, KeyType keyType)
        {
            return CreateKey($"{keyType}{divider}{keyValidator.ValidateKey(key)}");
        }

        /// <summary>
        /// Unwraps a key.
        /// </summary>
        public string UnwrapKey(string wrappedKey, KeyType keyType)
        {
            var lastElement = wrappedKey.Split(divider).Last();
            return lastElement.Substring(0, lastElement.Length - postfix.Length);
        }

        /// <summary>
        /// Gets the wrap pattern for a specific key type.
        /// </summary>
        public string WrapKeyPattern(KeyType keyType)
        {
            return CreatePattern($"{keyType}{divider}");
        }


        /// <summary>
        /// Get a wrap pattern for keys using wildcards.
        /// </summary>
        public string WrapKeyPattern(char wildcard)
        {
            return CreatePattern($"{new string(wildcard, 3)}{divider}");
        }

        #endregion

        #region Lists

        /// <summary>
        /// Wraps a list element key.
        /// </summary>
        public string WrapListElementKey(string key, int listElementKey)
        {
            return CreateKey($"{KeyType.lstx}{divider}{keyValidator.ValidateKey(key)}{divider}{listElementKey.ToString("D10")}");
        }

        /// <summary>
        /// Unwraps a list element key.
        /// </summary>
        public int UnwrapListElementKey(string wrappedKey)
        {
            var lastElement = wrappedKey.Split(divider).Last();
            return int.Parse(lastElement.Substring(0, lastElement.Length - postfix.Length));
        }

        /// <summary>
        /// Gets the list wrap pattern.
        /// </summary>
        public string WrapListElementPattern(string key)
        {
            return CreatePattern($"{KeyType.lstx}{divider}{keyValidator.ValidateKey(key)}");
        }

        #endregion

        /// <summary>
        /// Wraps a dictionary element key.
        /// </summary>
        public string WrapDictionaryElementKey(string key, string dictionaryElementKey)
        {
            return CreateKey($"{KeyType.dicx}{divider}{keyValidator.ValidateKey(key)}{divider}{keyValidator.ValidateDictionaryElementKey(dictionaryElementKey)}");
        }

        /// <summary>
        /// Unwraps a dictionary element key.
        /// </summary>
        public string UnwrapDictionarytElementKey(string wrappedKey)
        {
            var lastElement = wrappedKey.Split(divider).Last();
            return lastElement.Substring(0, lastElement.Length - postfix.Length);
        }

        /// <summary>
        /// Gets the dictionary element pattern.
        /// </summary>
        public string GetDictionaryElementPattern(string key)
        {
            return CreatePattern($"{KeyType.dicx}{divider}{keyValidator.ValidateKey(key)}");
        }

        private string CreateKey(string key)
        {
            return $"{@namespace}{divider}{key}{postfix}";
        }

        private string CreatePattern(string key)
        {
            return $"{@namespace}{divider}{key}{wildcard}{postfix}";
        }
    }
}