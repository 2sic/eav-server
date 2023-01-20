using System;
using ToSic.Eav.Context;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

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

        /// <summary>
        /// Helper class, so inheriting stuff doesn't need to update the constructor all the time
        /// </summary>
        [PrivateApi]
        public class AppDependencies: ServiceDependencies
        {
            public Generator<Query> QueryGenerator { get; }
            public ILazySvc<QueryManager> QueryManager { get; }
            internal readonly IZoneMapper ZoneMapper;
            internal readonly ISite Site;
            internal readonly IAppStates AppStates;
            internal readonly DataSourceFactory DataSourceFactory;

            public AppDependencies(IZoneMapper zoneMapper,
                ISite site,
                IAppStates appStates,
                DataSourceFactory dataSourceFactory,
                ILazySvc<QueryManager> queryManager,
                Generator<Query> queryGenerator)
            {
                AddToLogQueue(
                    ZoneMapper = zoneMapper,
                    Site = site,
                    AppStates = appStates,
                    DataSourceFactory = dataSourceFactory,
                    QueryManager = queryManager,
                    QueryGenerator = queryGenerator
                );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencies">All the dependencies of this app, managed by this app</param>
        /// <param name="logName">must be null by default, because of DI</param>
        public App(AppDependencies dependencies, string logName = null): base(logName ?? "Eav.App")
        {
            Deps = dependencies.SetLog(Log);
            _dsFactory = dependencies.DataSourceFactory;
            
            Site = dependencies.Site;
        }
        private readonly AppDependencies Deps;
        private readonly DataSourceFactory _dsFactory;


        #endregion

        /// <inheritdoc />
        public string Name { get; private set; }
        /// <inheritdoc />
        public string Folder { get; private set; }
        /// <inheritdoc />
        public bool Hidden { get; private set; }

        /// <inheritdoc />
        public string NameId { get; private set; }

        /// <inheritdoc />
        [Obsolete]
        [PrivateApi]
        public string AppGuid => NameId;

        /// <inheritdoc />
        public bool ShowDrafts { get; private set; }

        protected internal App Init(IAppIdentity appIdentity, Func<App, IAppDataConfiguration> buildConfiguration)
        {
            // Env / Tenant must be re-checked here
            if (Site == null) throw new Exception("no site/portal received");
            
            // in case the DI gave a bad tenant, try to look up
            if (Site.Id == Constants.NullId && appIdentity.AppId != Constants.NullId &&
                appIdentity.AppId != AppConstants.AppIdNotFound)
                Site = Deps.ZoneMapper.SiteOfApp(appIdentity.AppId);

            // if zone is missing, try to find it - but always assume current context
            if (appIdentity.ZoneId == AppConstants.AutoLookupZone)
                appIdentity = new AppIdentity(Site.ZoneId, appIdentity.AppId);

            Init(appIdentity);
            Log.A($"prep App #{appIdentity.Show()}, hasDataConfig:{buildConfiguration != null}");

            // Look up name in cache
            // 2020-11-25 changed to use State.Get. before it was this...
            //AppGuid = State.Cache.Zones[ZoneId].Apps[AppId];
            NameId = Deps.AppStates.Get(this).NameId;

            InitializeResourcesSettingsAndMetadata();

            // for deferred initialization as needed
            _dataConfigurationBuilder = buildConfiguration;

            return this;
        }
    }
}
