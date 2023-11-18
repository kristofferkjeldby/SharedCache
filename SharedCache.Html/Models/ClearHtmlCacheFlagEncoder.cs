namespace SharedCache.Html.Models
{
    using System;

    /// <summary>
    /// Clear cache encoded time
    /// </summary>
    public static class ClearHtmlCacheFlagEncoder
    {
        /// <summary>
        /// Encodes the specified clear HTML cache flag.
        /// </summary>
        /// <param name="clearHtmlCacheFlag">The clear HTML cache flag.</param>
        public static DateTime Encode(ClearHtmlCacheFlag clearHtmlCacheFlag)
        {
            var now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, (int)clearHtmlCacheFlag).ToUniversalTime();
        }

        /// <summary>
        /// Encodes the specified include all sites.
        /// </summary>
        /// <param name="includeAllSites">if set to <c>true</c> [include all sites].</param>
        /// <param name="clearContentHtmlCache">if set to <c>true</c> [clear content HTML cache].</param>
        /// <param name="clearStaticHtmlCache">if set to <c>true</c> [clear static HTML cache].</param>
        public static DateTime Encode(bool includeAllSites, bool clearContentHtmlCache, bool clearStaticHtmlCache)
        {
            var clearHtmlCacheFlag = (includeAllSites ? ClearHtmlCacheFlag.IncludeAllSites : ClearHtmlCacheFlag.None) | (clearContentHtmlCache ? ClearHtmlCacheFlag.ContentHtmlCache : ClearHtmlCacheFlag.None) | (clearStaticHtmlCache ? ClearHtmlCacheFlag.StaticHtmlCache : ClearHtmlCacheFlag.None);
            return Encode(clearHtmlCacheFlag);
        }

        /// <summary>
        /// Decodes the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        public static ClearHtmlCacheFlag Decode(DateTime dateTime)
        {
            // Note that will be true even for combinations of flags that do not have a string representation
            if (Enum.TryParse<ClearHtmlCacheFlag>(dateTime.Millisecond.ToString(), out var clearHtmlCacheFlag))
            {
                if (IsFlagsDefined(clearHtmlCacheFlag))
                    return clearHtmlCacheFlag;
            }

            return ClearHtmlCacheFlag.ContentHtmlCache;
        }

        /// <summary>
        /// Determines whether [is flags defined].
        /// </summary>
        /// <param name="flags">The flags.</param>
        private static bool IsFlagsDefined(this Enum flags)
        {
            char firstDigit = flags.ToString()[0];
            if (char.IsDigit(firstDigit) || firstDigit == '-')
                return false;

            return true;
        }
    }
}