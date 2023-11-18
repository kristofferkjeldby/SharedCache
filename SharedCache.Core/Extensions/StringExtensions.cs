namespace SharedCache.Core.Extensions
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Add expire to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="timeSpan">The time span.</param>
        public static string AddExpire(this string value, TimeSpan? timeSpan = null)
        {
            var dateTime = (timeSpan.HasValue) ? DateTime.Now.Add(timeSpan.Value) :
                DateTime.MaxValue;
            return string.Concat(value, Constants.DateTimeDelimiter, dateTime.ToUniversalTime().ToString(Constants.DateTimeFormat));
        }

        /// <summary>
        /// Removes expire from a string.
        /// </summary>
        /// <param name="encodedValue">The encoded value.</param>
        /// <param name="isExpired">if set to <c>true</c> [is expired].</param>
        public static string RemoveExpire(this string encodedValue, out bool isExpired)
        {
            var value = encodedValue.Substring(0, encodedValue.LastIndexOf(Constants.DateTimeDelimiter));
            var expire = DateTime.Parse(encodedValue.Substring(encodedValue.LastIndexOf(Constants.DateTimeDelimiter) + 1)).ToUniversalTime();
            isExpired = expire < DateTime.UtcNow;
            return value;
        }

        /// <summary>
        /// Hashes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string Hash(this string value)
        {
            byte[] bytes;

            using (HashAlgorithm algorithm = SHA256.Create())
            {
                bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in bytes)
                stringBuilder.Append(b.ToString("X2"));

            return stringBuilder.ToString();
        }
    }
}