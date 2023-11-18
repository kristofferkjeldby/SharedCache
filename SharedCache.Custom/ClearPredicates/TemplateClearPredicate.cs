namespace SharedCache.Custom.ClearPredicates
{
    using SharedCache.Custom.Extensions;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Events;
    using System.Linq;

    /// <summary>
    /// Encapsulate the clear predicate logic for custom caches based on templates
    /// </summary>
    public class TemplateClearPredicate : IClearPredicate
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include derived templates.
        /// </summary>
        public bool IncludeDerivedTemplates { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to clear on global publish.
        /// </summary>
        public bool ClearOnGlobal { get; }

        /// <summary>
        /// Gets or sets the predicates template ids.
        /// </summary>
        public ID[] PredicateTemplateIds { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateClearPredicate"/> class.
        /// </summary>
        public TemplateClearPredicate(bool clearOnGlobalPublish, bool includeDerivedTemplates, params ID[] predicateTemplatesIds)
        {
            this.ClearOnGlobal = clearOnGlobalPublish;
            this.IncludeDerivedTemplates = includeDerivedTemplates;
            this.PredicateTemplateIds = predicateTemplatesIds;
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public bool Execute(SitecoreEventArgs args)
        {
            if (!(args.Parameters[1] is ItemChanges itemChanges))
                return false;

            if (PredicateTemplateIds == null)
                return true;

            if (IncludeDerivedTemplates)
                return this.PredicateTemplateIds.Any(tid => itemChanges.Item.IsDerived(tid));

            return this.PredicateTemplateIds.Contains(itemChanges.Item.TemplateID);
        }
    }
}