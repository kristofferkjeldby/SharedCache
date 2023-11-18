namespace SharedCache.Html.Helpers
{
    using SharedCache.Html.Models;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Publishing;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper class for building the publishing Options
    /// </summary>
    public static class PublishingOptionsBuilder
    {

        /// <summary>
        /// Builds the publishing options.
        /// </summary>
        /// <param name="sourceDatabase">The source database.</param>
        /// <param name="rootItem">The root item.</param>
        /// <param name="targetDatabases">The target databases.</param>
        /// <param name="languages">The languages.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="deep">if set to <c>true</c> [deep].</param>
        /// <param name="compareRevisions">if set to <c>true</c> [compare revisions].</param>
        /// <param name="publishRelatedItems">if set to <c>true</c> [publish related items].</param>
        /// <param name="includeAllSites">if set to <c>true</c> [include all sites].</param>
        /// <param name="clearContentHtmlCache">if set to <c>true</c> [clear content HTML cache].</param>
        /// <param name="clearStaticHtmlCache">if set to <c>true</c> [clear static HTML cache].</param>
         public static PublishOptions[] BuildOptions(Database sourceDatabase, Item rootItem, Database[] targetDatabases, Language[] languages, PublishMode mode, bool deep, bool compareRevisions, bool publishRelatedItems, bool includeAllSites, bool clearContentHtmlCache, bool clearStaticHtmlCache)
        {
            Assert.ArgumentNotNull(sourceDatabase, nameof(sourceDatabase));
            Assert.ArgumentNotNull(targetDatabases, nameof(targetDatabases));
            Assert.ArgumentNotNull(languages, nameof(languages));

            var allPublishOptions = new List<PublishOptions>();

            foreach (Database targetDatabase in targetDatabases)
            {
                foreach (Language language in languages)
                {
                    DateTime utcNow = ClearHtmlCacheFlagEncoder.Encode(includeAllSites, clearContentHtmlCache, clearStaticHtmlCache);
                    Language languageInSourceDatabase = sourceDatabase.Languages.FirstOrDefault(l => l.Name.Equals(language.Name)) ?? language;
                    PublishOptions publishOptions = new PublishOptions(sourceDatabase, targetDatabase, mode, languageInSourceDatabase, utcNow)
                    {
                        Deep = deep,
                        CompareRevisions = compareRevisions,
                        PublishRelatedItems = publishRelatedItems
                    };

                    if (rootItem != null)
                        publishOptions.RootItem = rootItem;

                    allPublishOptions.Add(publishOptions);

                }
            }

            return allPublishOptions.ToArray();
        }
    }
}