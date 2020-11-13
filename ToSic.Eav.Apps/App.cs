using System;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Queries;
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

        [PrivateApi]
        protected DataSourceFactory DataSourceFactory { get; }

        /// <summary>
        /// Helper class, so inheriting stuff doesn't need to update the constructor all the time
        /// </summary>
        [PrivateApi]
        public class AppDependencies
        {
            //internal AppInitializedChecker InitializedChecker { get; }
            internal readonly IZoneMapper ZoneMapper;
            internal readonly IEnvironment Environment;
            internal readonly ISite Site;
            internal readonly DataSourceFactory DataSourceFactory;
            internal readonly Lazy<GlobalQueries> GlobalQueriesLazy;

            public AppDependencies(
                IEnvironment environment,
                IZoneMapper zoneMapper,
                ISite site,
                DataSourceFactory dataSourceFactory,
                Lazy<GlobalQueries> globalQueriesLazy)
            {
                ZoneMapper = zoneMapper;
                Environment = environment;
                Site = site;
                DataSourceFactory = dataSourceFactory;
                GlobalQueriesLazy = globalQueriesLazy;
            }
        }
        [PrivateApi]
        private readonly AppDependencies _dependencies;

        public App(AppDependencies dependencies, string logName): base(logName ?? "Eav.App", new CodeRef())
        {
            _dependencies = dependencies;
            dependencies.ZoneMapper.Init(Log);
            DataSourceFactory = dependencies.DataSourceFactory;
            // just keep pointers for now, don't init/verify yet
            // as in some cases (like search) they will be replaced after the constructor
            Env = dependencies.Environment;
            Env.Init(Log);
            Site = dependencies.Site;
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

        [PrivateApi]
        protected const string IconFile = "/" + AppConstants.AppIconFile;




        protected internal App Init(IAppIdentity appIdentity, bool allowSideEffects, Func<App, IAppDataConfiguration> buildConfiguration, ILog parentLog)
        {
            // Env / Tenant must be re-checked here
            if (Site == null) throw new Exception("no site/portal received");
            
            // in case the DI gave a bad tenant, try to look up
            if (Site.Id == Constants.NullId && appIdentity.AppId != Constants.NullId &&
                appIdentity.AppId != AppConstants.AppIdNotFound)
                Site = _dependencies.ZoneMapper.TenantOfApp(appIdentity.AppId);

            // if zone is missing, try to find it; if still missing, throw error
            if (appIdentity.ZoneId == AutoLookupZone)
                appIdentity = _dependencies.ZoneMapper.IdentityFromSite(Site.Id, appIdentity.AppId);

            Init(appIdentity, new CodeRef(), parentLog);
            Log.Add($"prep App #{appIdentity.Show()}, allowSE:{allowSideEffects}, hasDataConfig:{buildConfiguration != null}");

            // Look up name in cache
            AppGuid = State.Cache.Zones[ZoneId].Apps[AppId];

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
