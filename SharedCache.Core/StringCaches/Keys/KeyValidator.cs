namespace SharedCache.Core.StringCaches.Keys
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// An implementation of a key validator. The key validator will check the keys used by a string cache to prevent the use of
    /// reserved chars.
    /// </summary>
    public class KeyValidator
    {
        private readonly KeyValidationMode keyValidationMode;
        private readonly KeyValidationMode dictionaryKeyValidationMode;
        private readonly IEnumerable<char> illegalChars;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValidator"/> class.
        /// </summary>
        /// <param name="keyValidationMode">The key validation mode.</param>
        /// <param name="dictionaryKeyValidationMode">The dictionary key validation mode.</param>
        /// <param name="illegalChars">The illegal chars.</param>
        public KeyValidator(KeyValidationMode keyValidationMode, KeyValidationMode dictionaryKeyValidationMode, IEnumerable<char> illegalChars)
        {
            if (new Regex(@"[0-9A-Za-z]").IsMatch(new string(illegalChars.ToArray())))
                throw new Exception("All string caches must support numbers and letters");

            this.keyValidationMode = keyValidationMode;
            this.dictionaryKeyValidationMode = dictionaryKeyValidationMode;
            this.illegalChars = illegalChars;
        }

        #region Keys

        /// <summary>
        /// Validates a key.
        /// </summary>
        public string ValidateKey(string key)
        {
            return Validate(key, keyValidationMode);
        }

        #endregion

        #region Dictionaries

        /// <summary>
        /// Validates a dictionary key.
        /// </summary>
        public string ValidateDictionaryElementKey(string directoryKey)
        {
            return Validate(directoryKey, dictionaryKeyValidationMode);
        }

        #endregion

        private string Validate(string value, KeyValidationMode keyValidationMode)
        {
            switch (keyValidationMode)
            {
                case KeyValidationMode.NoCheck:
                    return value;
                case KeyValidationMode.CheckAndWarn:
                    if (ContainsIllegalChars(value))
                        Sitecore.Diagnostics.Log.Warn($"Value {value} contains one or more illegal char ({string.Join(",", illegalChars)})", this);
                    return value;
                case KeyValidationMode.CheckAndFail:
                    if (ContainsIllegalChars(value))
                        throw new Exception($"Value {value} contains one or more illegal char ({string.Join(",", illegalChars)})");
                    return value;
            }

            return value;
        }

        /// <summary>
        /// Checks a key for an illegal character.
        /// </summary>
        public bool ContainsIllegalChars(string key)
        {
            return key.Any(IsIllegalChar);
        }

        /// <summary>
        /// Checks a key for a list of illegal characters.
        /// </summary>
        public bool IsIllegalChars(IEnumerable<char> chars)
        {
            return chars.All(IsIllegalChar);
        }

        /// <summary>
        /// Checks a character.
        /// </summary>
        public bool IsIllegalChar(char @char)
        {
            return illegalChars.Contains(@char);
        }
    }
}