namespace SharedCache.Custom.ClearPredicates
{
    using SharedCache.Custom.Extensions;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using System.Linq;

    /// <summary>
    /// Encapsulate the clear predicate logic for custom caches based on templates
    /// </summary>
    public class TemplateClearPredicate : ClearPredicate
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include derived templates.
        /// </summary>
        public bool IncludeDerivedTemplates { get; }

        /// <inheritdoc/>
        public override bool ClearOnGlobal { get; }

        /// <inheritdoc/>
        public override bool UseSiteNameAsCacheKey { get; }

        /// <summary>
        /// Gets or sets the predicates template ids.
        /// </summary>
        public ID[] PredicateTemplateIds { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateClearPredicate"/> class.
        /// </summary>
        public TemplateClearPredicate(bool clearOnGlobalPublish, bool useSiteNameAsCacheKey, bool includeDerivedTemplates, params ID[] predicateTemplatesIds)
        {
            this.ClearOnGlobal = clearOnGlobalPublish;
            this.UseSiteNameAsCacheKey = useSiteNameAsCacheKey; 
            this.IncludeDerivedTemplates = includeDerivedTemplates;
            this.PredicateTemplateIds = predicateTemplatesIds;
        }

        /// <inheritdoc/>
        public override bool DoClear(Item item)
        {

            if (item == null)
                return false;

            if (PredicateTemplateIds == null)
                return false;

            if (IncludeDerivedTemplates)
                return this.PredicateTemplateIds.Any(tid => item.IsDerived(tid));

            return this.PredicateTemplateIds.Contains(item.TemplateID);
        }
    }
}