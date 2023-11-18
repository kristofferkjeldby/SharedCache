namespace SharedCache.Html.Dialogs
{
    using SharedCache.Html.Helpers;
    using Sitecore;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Publishing;
    using Sitecore.Shell;
    using Sitecore.Shell.Applications.Dialogs.Publish;
    using Sitecore.Text;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Sheer;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Cache publish porm
    /// </summary>
    public class CachePublishForm : PublishForm
    {
        /// <summary>
        /// The clear content html cache checkbox
        /// </summary>
        protected Checkbox ClearContentHtmlCache;

        /// <summary>
        /// The clear static html cache checkbox
        /// </summary>
        protected Checkbox ClearStaticHtmlCache;

        /// <summary>
        /// The include all sites checkbox
        /// </summary>
        protected Checkbox IncludeAllSites;

        /// <summary>
        /// The include all sites pane
        /// </summary>
        protected Border IncludeAllSitesPane;

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            IncludeAllSitesPane.Visible = Context.User.IsAdministrator;
            if (string.IsNullOrEmpty(this.ItemID))
            {
                this.ClearContentHtmlCache.Checked = true;
                this.ClearStaticHtmlCache.Checked = true;
                this.IncludeAllSites.Checked = true;
            }
            else
            {
                this.SmartPublish.Checked = true;
            }
        }

        /// <summary>
        /// Starts the publisher.
        /// </summary>
        protected new void StartPublisher()
        {
            Language[] languages = GetLanguages();
            List<Item> publishingTargets = GetPublishingTargets();
            Database[] publishingTargetDatabases = GetPublishingTargetDatabases();
            bool incrementalPublish = Context.ClientPage.ClientRequest.Form["PublishMode"] == "IncrementalPublish";
            bool smartPublish = Context.ClientPage.ClientRequest.Form["PublishMode"] == "SmartPublish";
            bool republish = Context.ClientPage.ClientRequest.Form["PublishMode"] == "Republish";
            bool rebuild = this.Rebuild;
            bool publishChildren = this.PublishChildren.Checked;
            bool publishRelatedItems = this.PublishRelatedItems.Checked;
            string itemId = this.ItemID;

            if (string.IsNullOrEmpty(itemId))
                itemId = "null";

            string message;

            if (rebuild)
                message = string.Format("Rebuild database, databases: {0}", (object)StringUtil.Join(publishingTargetDatabases, ", "));
            else
                message = string.Format("Publish, root: {0}, languages:{1}, targets:{2}, databases:{3}, incremental:{4}, smart:{5}, republish:{6}, children:{7}, related:{8}, clear content html cache:{9}, clear static html cache:{10}", new object[11]
                {
                    itemId,
                    StringUtil.Join(languages, ", "),
                    StringUtil.Join(publishingTargets, ", ", "Name"),
                    StringUtil.Join(publishingTargetDatabases, ", "),
                    MainUtil.BoolToString(incrementalPublish),
                    MainUtil.BoolToString(smartPublish),
                    MainUtil.BoolToString(republish),
                    MainUtil.BoolToString(publishChildren),
                    MainUtil.BoolToString(publishRelatedItems),
                    MainUtil.BoolToString(ClearContentHtmlCache.Checked),
                    MainUtil.BoolToString(ClearStaticHtmlCache.Checked)
                }
            );

            Log.Audit(message, this.GetType());

            ListString languageString = new ListString();
            foreach (Language language in languages)
                languageString.Add(language.ToString());
            Registry.SetString("/Current_User/Publish/Languages", languageString.ToString());

            ListString targetString = new ListString();
            foreach (Item obj in publishingTargets)
                targetString.Add(obj.ID.ToString());
            Registry.SetString("/Current_User/Publish/Targets", targetString.ToString());

            UserOptions.Publishing.IncrementalPublish = incrementalPublish;
            UserOptions.Publishing.SmartPublish = smartPublish;
            UserOptions.Publishing.Republish = republish;
            UserOptions.Publishing.PublishChildren = publishChildren;
            UserOptions.Publishing.PublishRelatedItems = publishRelatedItems;

            // This deals with single item publish
            if (!string.IsNullOrEmpty(this.ItemID))
            {
                var publishOptions = BuildPublishItemPublishOption(Client.GetItemNotNull(ItemID), publishingTargetDatabases, languages, publishChildren, smartPublish, publishRelatedItems);

                this.JobHandle = PublishManager.Publish(publishOptions).ToString();
            }
            // These are all variations of full publishes
            else
            {
                this.JobHandle = (
                        (!incrementalPublish ?
                            (!smartPublish ?
                                (!rebuild ?
                                PublishManager.Republish(Client.ContentDatabase, publishingTargetDatabases, languages, Context.Language) :
                                PublishManager.RebuildDatabase(Client.ContentDatabase, publishingTargetDatabases)) :
                                PublishManager.PublishSmart(Client.ContentDatabase, publishingTargetDatabases, languages, Context.Language)) :
                                PublishManager.Publish(BuildOptionsIncrementalPublish(Client.ContentDatabase, publishingTargetDatabases, languages))).ToString());
            }
            
            SheerResponse.Timer("CheckStatus", Settings.Publishing.PublishDialogPollingInterval);
        }

        /// <summary>
        /// Build publish options for Incremental Publish.
        /// </summary>
        private PublishOptions[] BuildOptionsIncrementalPublish(Database source, Database[] targets, Language[] languages)
        {
            return PublishingOptionsBuilder.BuildOptions(
                source,
                null,
                targets,
                languages,
                PublishMode.Incremental,
                false,
                false,
                false,
                IncludeAllSites.Checked, 
                this.ClearContentHtmlCache.Checked, 
                this.ClearStaticHtmlCache.Checked
                );
        }

        
        /// <summary>
        /// Build publish options for item publish.
        /// </summary>
        private PublishOptions[] BuildPublishItemPublishOption(Item item, Database[] targets, Language[] languages, bool deep, bool compareRevisions, bool publishRelatedItems)
        { 
            return PublishingOptionsBuilder.BuildOptions(
                item.Database, 
                item, 
                targets, 
                languages, 
                PublishMode.SingleItem, 
                deep, 
                compareRevisions, 
                publishRelatedItems, 
                this.IncludeAllSites.Checked, 
                this.ClearContentHtmlCache.Checked, 
                this.ClearStaticHtmlCache.Checked
                );
        }

        /// <summary>
        /// Gets the languages.
        /// </summary>
        private static Language[] GetLanguages()
        {
            ArrayList arrayList = new ArrayList();
            foreach (string key in Context.ClientPage.ClientRequest.Form.Keys)
            {
                if (key != null && key.StartsWith("la_", StringComparison.InvariantCulture))
                    arrayList.Add((object)Language.Parse(Context.ClientPage.ClientRequest.Form[key]));
            }
            return arrayList.ToArray(typeof(Language)) as Language[];
        }

        /// <summary>
        /// Gets the publishing target databases.
        /// </summary>
        private static Database[] GetPublishingTargetDatabases()
        {
            ArrayList arrayList = new ArrayList();
            foreach (BaseItem publishingTarget in GetPublishingTargets())
            {
                string name = publishingTarget["Target database"];
                Database database = Factory.GetDatabase(name);
                Assert.IsNotNull((object)database, typeof(Database), Translate.Text("Database \"{0}\" not found."), name);
                arrayList.Add((object)database);
            }
            return arrayList.ToArray(typeof(Database)) as Database[];
        }

        /// <summary>
        /// Gets the publishing targets.
        /// </summary>
        private static List<Item> GetPublishingTargets()
        {
            List<Item> objList = new List<Item>();
            foreach (string key in Context.ClientPage.ClientRequest.Form.Keys)
            {
                if (key != null && key.StartsWith("pb_", StringComparison.InvariantCulture))
                {
                    Item obj = Context.ContentDatabase.Items[ShortID.Decode(key.Substring(3))];
                    Assert.IsNotNull((object)obj, typeof(Item), "Publishing target not found.", Array.Empty<object>());
                    objList.Add(obj);
                }
            }
            return objList;
        }
    }
}