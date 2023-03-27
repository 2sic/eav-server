using System;
using ToSic.Eav.Context;
using ToSic.Eav.DataSource.Query;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Services;
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
    public partial class App: AppBase<App.MyServices>, IApp
    {
        #region Constructor / DI

        /// <summary>
        /// Helper class, so inheriting stuff doesn't need to update the constructor all the time
        /// </summary>
        [PrivateApi]
        public class MyServices: MyServicesBase
        {
            public Generator<Query> QueryGenerator { get; }
            public LazySvc<QueryManager> QueryManager { get; }
            internal readonly IZoneMapper ZoneMapper;
            internal readonly ISite Site;
            internal readonly IAppStates AppStates;
            internal readonly IDataSourcesService DataSourceFactory;

            public MyServices(IZoneMapper zoneMapper,
                ISite site,
                IAppStates appStates,
                IDataSourcesService dataSourceFactory,
                LazySvc<QueryManager> queryManager,
                Generator<Query> queryGenerator)
            {
                ConnectServices(
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
        /// <param name="services">All the dependencies of this app, managed by this app</param>
        /// <param name="logName">must be null by default, because of DI</param>
        public App(MyServices services, string logName = null): base(services, logName ?? "Eav.App")
        {
            _dsFactory = services.DataSourceFactory;
            
            Site = services.Site;
        }
        private readonly IDataSourcesService _dsFactory;


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

        protected internal App Init(IAppIdentity appIdentity, Func<App, IAppDataConfiguration> buildConfiguration)
        {
            // Env / Tenant must be re-checked here
            if (Site == null) throw new Exception("no site/portal received");
            
            // in case the DI gave a bad tenant, try to look up
            if (Site.Id == Constants.NullId && appIdentity.AppId != Constants.NullId &&
                appIdentity.AppId != AppConstants.AppIdNotFound)
                Site = Services.ZoneMapper.SiteOfApp(appIdentity.AppId);

            // if zone is missing, try to find it - but always assume current context
            if (appIdentity.ZoneId == AppConstants.AutoLookupZone)
                appIdentity = new AppIdentity(Site.ZoneId, appIdentity.AppId);
            
            base.Init(appIdentity);
            Log.A($"prep App #{appIdentity.Show()}, hasDataConfig:{buildConfiguration != null}");

            // Look up name in cache
            NameId = Services.AppStates.Get(this).NameId;

            InitializeResourcesSettingsAndMetadata();

            // for deferred initialization as needed
            _dataConfigurationBuilder = buildConfiguration;

            return this;
        }
    }
}
