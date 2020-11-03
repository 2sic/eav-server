using System;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// A <em>single-use</em> app-object providing quick simple api to access
    /// name, folder, data, metadata etc.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public partial class App: AppBase, IApp
    {
        #region Constructor / DI

        protected DataSourceFactory DataSourceFactory { get; }

        public App(IAppEnvironment environment, ISite site, DataSourceFactory dataSourceFactory, string logName): base(logName ?? "Eav.App", new CodeRef())
        {
            DataSourceFactory = dataSourceFactory;
            // just keep pointers for now, don't init/verify yet
            // as in some cases (like search) they will be replaced after the constructor
            Env = environment;
            Site = site;
        }

        #endregion


        [PrivateApi]
        public const int AutoLookupZone = -1;

        /// <inheritdoc />
        public string Name { get; private set; }
        /// <inheritdoc />
        public string Folder { get; private set; }
        /// <inheritdoc />
        public bool Hidden { get; private set; }

        /// <inheritdoc />
        public string AppGuid { get; private set; }

        /// <inheritdoc />
        public bool ShowDrafts { get; private set; }
        /// <inheritdoc />
        public bool EnablePublishing { get; private set; }

        [PrivateApi]
        protected const string IconFile = "/" + AppConstants.AppIconFile;




        protected internal App Init(IAppIdentity appIdentity, bool allowSideEffects, Func<App, IAppDataConfiguration> buildConfiguration, ILog parentLog)
        {
            // Env / Tenant must be re-checked here
            Env = Env ?? throw new Exception("no environment received");
            Env.Init(parentLog);
            if (Site == null) throw new Exception("no site/portal received");
            
            // in case the DI gave a bad tenant, try to look up
            if (Site.Id == Constants.NullId && appIdentity.AppId != Constants.NullId &&
                appIdentity.AppId != AppConstants.AppIdNotFound)
                Site = Env.ZoneMapper.TenantOfApp(appIdentity.AppId);

            // if zone is missing, try to find it; if still missing, throw error
            if (appIdentity.ZoneId == AutoLookupZone)
                appIdentity = Env.ZoneMapper.IdentityFromSite(Site.Id, appIdentity.AppId);

            Init(appIdentity, new CodeRef(), parentLog);
            Log.Add($"prep App #{appIdentity.ZoneId}/{appIdentity.AppId}, allowSE:{allowSideEffects}, hasDataConfig:{buildConfiguration != null}");

            // Look up name in cache
            var cache = State.Cache;
            AppState = cache.Get(this); // for metadata

            AppGuid = cache.Zones[ZoneId].Apps[AppId];

            // v10.25 from now on the DefaultApp can also have settings and resources
            // v10.26.0x reactivated this protection, because it causes side-effects. On content-app, let's only do this if people start editing the resources...?
            // note that on imported apps, this would automatically work, as those would already have these things
            if (AppGuid != Constants.DefaultAppName)
            {
                // if it's a real App (not content/default), do more
                Log.Add($"create app resources? allowSE:{allowSideEffects}");

                if (allowSideEffects)
                    Factory.Resolve<AppManager>().Init(this, Log).EnsureAppIsConfigured(); // make sure additional settings etc. exist
            }

            InitializeResourcesSettingsAndMetadata();

            // do this after initializing resources to certainly set the content-name
            if(AppGuid == Constants.DefaultAppName)
                Name = Folder = Constants.ContentAppName;

            // for deferred initialization as needed
            _dataConfigurationBuilder = buildConfiguration;

            return this;
        }
    }
}
