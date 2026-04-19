using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using PerformantSitecore.Foundation.SxaSiteProvider.Extensions;
using PerformantSitecore.Foundation.SxaSiteProvider.Helpers;
using PerformantSitecore.Foundation.SxaSiteProvider.Pipeline.SiteProviderInitialized;

using Sitecore;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.SecurityModel;
using Sitecore.SecurityModel.License;
using Sitecore.Sites;
using Sitecore.Web;
using Sitecore.XA.Foundation.Multisite;
using Sitecore.XA.Foundation.Multisite.Providers;
using Sitecore.XA.Foundation.Multisite.SiteResolvers;

using Microsoft.Extensions.DependencyInjection;

using StringDictionary = Sitecore.Collections.StringDictionary;
using Templates = Sitecore.XA.Foundation.Multisite.Templates;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Providers
{
    /// <summary>
    /// An asynchronous replacement for the default SXA SiteProvider.
    /// Site definition reloads happen in a background Sitecore job, so
    /// editors are never blocked while sites are being refreshed.
    /// The dirty-flag mechanism prevents duplicate jobs while ensuring
    /// no site definition change is missed.
    /// </summary>
    public class AsyncSxaSiteProvider : SiteProvider, ISxaSiteProvider, ISxaDatabaseSiteProvider,
        IAsyncSxaSiteProvider
    {
        private bool _init = false;
        private bool _dirty = false;
        private readonly bool _initialLoadAsync;
        private readonly object _lock = new object();
        private string _database;
        private SiteCollection _sites;
        private SafeDictionary<string, Site> _siteDictionary;
        private const string PreventCacheClear = "preventHtmlCacheClear";
        private readonly ILoggingHelper _logHelper;
        private readonly IPipelinesHelper _pipelinesHelper;
        private readonly IEnvironmentSitesResolver _sitesResolver;

        public Handle JobHandle
        {
            get;
            set;
        }

        public AsyncSxaSiteProvider(ISiteContextFactoryHelper siteContextFactoryHelper,
            IPipelinesHelper pipelinesHelper, ILoggingHelper logHelper, IEnvironmentSitesResolver sitesResolver)
        {
            _pipelinesHelper = pipelinesHelper;
            _logHelper = logHelper;
            _sitesResolver = sitesResolver;
            JobHandle = Handle.Null;
            _initialLoadAsync = Settings.GetBoolSetting(Constants.Settings.SxaSiteProvider.InitialLoadAsync, false);
        }


        public void StartBackgroundJob(Item item)
        {
            if (!IsReloading)
            {
                _logHelper.LogInfo($"AsyncSxaSiteProvider.StartBackgroundJob[{item?.Uri}]", this);
                DefaultJobOptions jobOptions =
                    new DefaultJobOptions("Site Provider Reloader",
                        "SxaSiteProvider",
                        "<all>",
                        this,
                        "InitializeSites");
                jobOptions.Item = item;
                jobOptions.ContextUser = Context.User;
                jobOptions.AtomicExecution = false;
                jobOptions.EnableSecurity = false;
                JobHandle = JobManager.Start(jobOptions).Handle;
            }
        }

        public string Database => _database;

        public override void Initialize(string name, NameValueCollection config)
        {
            Assert.ArgumentNotNullOrEmpty(name, "name");
            Assert.ArgumentNotNull(config, "config");
            base.Initialize(name, config);
            _database = config["database"];
        }

        public override Site GetSite(string siteName)
        {
            Assert.ArgumentNotNullOrEmpty(siteName, nameof(siteName));
            _logHelper.LogDebug($"GetSite[{siteName}]", this);
            TryBackgroundInitialize();
            return _siteDictionary?[siteName];
        }

        public override SiteCollection GetSites()
        {
            TryBackgroundInitialize();
            SiteCollection siteCollection = new SiteCollection();
            if (_sites != null)
            {
                siteCollection.AddRange(_sites);
            }

            return siteCollection;
        }

        private void TryBackgroundInitialize()
        {
            if (_siteDictionary == null)
            {
                if (_initialLoadAsync)
                {
                    _siteDictionary = new SafeDictionary<string, Site>();
                    _dirty = true;
                    StartBackgroundJob(null);
                }
                else
                {
                    InitializeSites();
                }
            }
            else if (_dirty)
            {
                StartBackgroundJob(null);
            }
        }

        public void Reset()
        {
            _dirty = true;
            TryBackgroundInitialize();
        }

        private void InitializeSites()
        {
            while (_init && _siteDictionary == null) { }

            if (!_dirty && _siteDictionary != null)
            {
                return;
            }

            try
            {
                _init = true;
                _logHelper.LogInfo($"AsyncSxaSiteProvider.InitializeSites[Start]", this);
                var siteList = GetSiteList();

                lock (_lock)
                {
                    if (_siteDictionary != null && !_dirty)
                    {
                        _init = false;
                        return;
                    }

                    _dirty = false;
                    var sites = new SiteCollection();
                    var siteDictionary = new SafeDictionary<string, Site>(StringComparer.OrdinalIgnoreCase);
                    foreach (var siteItem in siteList)
                    {
                        var i = 0;
                        string name;
                        using (new SecurityDisabler())
                        {
                            name = siteItem[Templates._BaseSiteDefinition.Fields.SiteName].Trim();
                        }

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            Log.Warn($"Empty site name for a definition: {siteItem.Paths.Path}. Skipped.", this);
                            continue;
                        }

                        while (siteDictionary.ContainsKey(name))
                        {
                            name = $"{name}-{++i}";
                        }

                        Site site;
                        using (new SecurityDisabler())
                        using (new EnforceVersionPresenceDisabler())
                        {
                            site = ParseSiteItem(siteItem, name);
                        }

                        if (site == null)
                        {
                            Log.Warn($"Incorrect site definition: {siteItem.Name}.", this);
                            continue;
                        }

                        sites.Add(site);
                        siteDictionary[name] = site;
                    }

                    _sites = sites;
                    _siteDictionary = siteDictionary;
                }

                _logHelper.LogInfo($"AsyncSxaSiteProvider.InitializeSites[PipelineStart]", this);
                var args = new SiteProviderInitializedPipelineArgs(siteList.FirstOrDefault());
                _pipelinesHelper.RunPipeline("siteProviderInitialized", args);
                _logHelper.LogInfo($"AsyncSxaSiteProvider.InitializeSites[Finished]", this);
            }
            finally
            {
                _init = false;
                JobHandle = Handle.Null;
            }
        }

        private List<Item> GetSiteList()
        {
            var database = Factory.GetDatabase(_database ?? string.Empty, false);
            List<Item> siteList;
            if (License.HasModule("Sitecore.SXA"))
            {
                using (new SecurityDisabler())
                {
                    siteList = _sitesResolver.ResolveAllSites(database).ToList();
                    siteList = _sitesResolver.ResolveEnvironmentSites(siteList, _sitesResolver.GetActiveEnvironment()).ToList();
                }
            }
            else
            {
                siteList = new List<Item>(0);
            }

            return siteList;
        }

        private Site ParseSiteItem(Item item, string name)
        {
            ReferenceField startItemField = item.Fields[Templates._BaseSiteDefinition.Fields.StartItem];
            if (startItemField == null)
            {
                return null;
            }

            Item homeItem = item.Database.GetItem(startItemField.TargetID);
            if (homeItem == null)
            {
                return null;
            }

            Item siteItem = ServiceLocator.ServiceProvider.GetService<IMultisiteContext>().GetSiteItem(homeItem);
            if (siteItem == null)
            {
                return null;
            }

            var siteDefinitionParser = ServiceLocator.ServiceProvider.GetService<ISiteDefinitionParser>();

            StringDictionary properties = new StringDictionary
            {
                {"name", name},
                {"hostName", item[Templates._BaseSiteDefinition.Fields.HostName].Replace(" ",string.Empty)},
                {"virtualFolder", item[Templates._BaseSiteDefinition.Fields.VirtualFolder].EnsurePrefix('/')},
                {"physicalFolder", "/"},
                {"rootPath", siteItem.Paths.FullPath},
                {"siteDefinitionPath", item.Paths.FullPath},
                {"startItem", homeItem.Paths.GetPath(siteItem.Paths.FullPath, "/", ItemPathType.Name)},
                {"database", item[Templates._BaseSiteDefinition.Fields.Database]},
                {"domain", item[Templates._BaseSiteDefinition.Fields.Domain]},
                {"language", item[Templates._BaseSiteDefinition.Fields.Language]},
                {"targetHostName", item[Templates._BaseSiteDefinition.Fields.TargetHostName].Replace(" ",string.Empty)},
                {"allowDebug", siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.AllowDebug])},
                {"cacheHtml", siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.CacheHtml])},
                {"enablePreview", siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.EnablePreview])},
                {"enableWebEdit", siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.EnableWebEdit])},
                {"enableDebugger", siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.EnableDebugger])},
                {"disableClientData", siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.DisableClientData])},
                {"languageEmbedding", siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.LanguageEmbedding])},
                {"linkProvider", item[Templates._BaseSiteDefinition.Fields.LinkProvider]},
                {"htmlCacheSize", siteDefinitionParser.ParseCacheSize(item.Fields[Templates._BaseSiteDefinition.Fields.HtmlCacheSize])},
                {"registryCacheSize", siteDefinitionParser.ParseCacheSize(item.Fields[Templates._BaseSiteDefinition.Fields.RegistryCacheSize])},
                {"viewStateCacheSize", siteDefinitionParser.ParseCacheSize(item.Fields[Templates._BaseSiteDefinition.Fields.ViewStateCacheSize])},
                {"xslCacheSize", siteDefinitionParser.ParseCacheSize(item.Fields[Templates._BaseSiteDefinition.Fields.XslCacheSize])},
                {"filteredItemsCacheSize", siteDefinitionParser.ParseCacheSize(item.Fields[Templates._BaseSiteDefinition.Fields.FilteredItemsCacheSize])},
                {"disableBrowserCaching", siteDefinitionParser.ParseTristate(item[Templates._BaseSiteDefinition.Fields.DisableBrowserCaching]).ToLowerInvariant()},
                {"IsSxaSite", "true"},
                {Sitecore.XA.Foundation.Multisite.Constants.SxaSiteTemplate, item.TemplateID.ToString()},
                {Sitecore.XA.Foundation.Multisite.Constants.IndexesSiteProperties, item[Templates.SiteDefinition.Fields.Indexes]},
                {"sxaLinkable", siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.SxaLinkable])},
            };

            ReferenceField loginItemField = item.Fields[Templates._BaseSiteDefinition.Fields.LoginPage];
            if (loginItemField != null)
            {
                Item loginPage = item.Database.GetItem(loginItemField.TargetID);
                if (loginPage != null)
                {
                    properties.Add("loginPage", GetItemPath(loginPage, homeItem, item[Templates._BaseSiteDefinition.Fields.VirtualFolder].EnsurePrefix('/')));
                    properties.Add("requireLogin", siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.RequireLogin]));
                }
            }

            NameValueCollection otherProperties = WebUtil.ParseUrlParameters(item[Templates._BaseSiteDefinition.Fields.OtherProperties] ?? string.Empty);
            foreach (string key in otherProperties)
            {
                if (!properties.ContainsKey(key))
                {
                    properties.Add(key, otherProperties[key]);
                }
            }

            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.EnableFieldLanguageFallback, properties, siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.FieldLanguageFallback]));
            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.EnableItemLanguageFallback, properties, siteDefinitionParser.ParseFlag(item.Fields[Templates._BaseSiteDefinition.Fields.ItemLanguageFallback]));

            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.ErrorPageUrl, properties, item.Fields[Templates._BaseSiteDefinition.Fields.ErrorPageUrl]?.Value);
            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.NoAccessUrl, properties, item.Fields[Templates._BaseSiteDefinition.Fields.NoAccessUrl]?.Value);
            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.NoLicenseUrl, properties, item.Fields[Templates._BaseSiteDefinition.Fields.NoLicenseUrl]?.Value);
            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.LayoutNotFoundUrl, properties, item.Fields[Templates._BaseSiteDefinition.Fields.LayoutNotFoundUrl]?.Value);
            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.ItemNotFoundUrl, properties, item.Fields[Templates._BaseSiteDefinition.Fields.ItemNotFoundUrl]?.Value);
            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.LinkItemNotFoundUrl, properties, item.Fields[Templates._BaseSiteDefinition.Fields.LinkItemNotFoundUrl]?.Value);
            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.NoPublishableUrl, properties, item.Fields[Templates._BaseSiteDefinition.Fields.NoPublishableUrl]?.Value);

            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.Port, properties, item.Fields[Templates._BaseSiteDefinition.Fields.Port]?.Value);
            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.ExternalPort, properties, item.Fields[Templates._BaseSiteDefinition.Fields.ExternalPort]?.Value);
            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.Scheme, properties, item.Fields[Templates._BaseSiteDefinition.Fields.Scheme]?.Value);

            AddIfUnspecified(Sitecore.XA.Foundation.Multisite.Constants.DictionaryDomain, properties, item.Fields[Templates.SiteDefinition.Fields.DictionaryDomain]?.Value);

            AddIfUnspecified(PreventCacheClear, properties, "true");
            AddIfUnspecified("enablePartialHtmlCacheClear", properties, "false");

            AddIfUnspecified("isInternal", properties, "false");

            return new Site(name, properties);
        }

        protected virtual void AddIfUnspecified(string key, StringDictionary properties, string value)
        {
            if (!properties.ContainsKey(key) && !string.IsNullOrWhiteSpace(value))
            {
                properties.Add(key, value);
            }
        }

        public virtual string GetItemPath(Item item, Item homeItem, string virtualFolder)
        {
            string path = item.Paths.GetPath(homeItem.Paths.FullPath, "/", ItemPathType.Name);
            virtualFolder = virtualFolder.TrimEnd('/');
            return virtualFolder + path;
        }

        public bool IsReloading
        {
            get
            {
                return !JobHandle.Equals(Handle.Null);
            }
        }
    }
}
