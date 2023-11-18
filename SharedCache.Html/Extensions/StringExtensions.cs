namespace SharedCache.Html.Extensions
{
    using System;

    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether this string starts with the value (case insensitive).
        /// </summary>
        public static bool StartsWithIgnoreCase(this string source, string value)
        {
            return source.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}