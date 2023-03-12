using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// All the data inside an App. <br/>
	/// For example, it has a variable amount of Out-streams, one for each content-type in the app.
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
		NiceName = "App",
		UiHint = "All data in an app with streams for type",
        Icon = Icons.TableChart,
        Type = DataSourceType.Source,
        GlobalName = "ToSic.Eav.DataSources.App, ToSic.Eav.DataSources",
        DynamicOut = true,
        In = new []{DataSourceConstants.DefaultStreamName},
		ExpectsDataOfType = "|Config ToSic.Eav.DataSources.App",
        HelpLink = "https://r.2sxc.org/DsApp")]
    public partial class App : DataSource
	{
        #region Configuration-properties

        private const int NotConfigured = 0;

        /// <summary>
        /// Use this to re-target the app-source to another app. <br/>
        /// Note that this can only be done before ever accessing the app - once the object has started reading data, switching has no more effect.
        /// </summary>
        [Configuration(Fallback = NotConfigured)]
        public int AppSwitch
        {
            get => Configuration.GetThis(NotConfigured);
            set
			{
                Configuration.SetThis(value);
                AppId = value;
				RequiresRebuildOfOut = true;
			}
		}

        /// <summary>
        /// Use this to re-target the app-source to another zone. <br/>
        /// Note that this can only be done before ever accessing the app - once the object has started reading data, switching has no more effect.
        /// </summary>
        [Configuration(Fallback = NotConfigured)]
		public int ZoneSwitch
        {
            get => Configuration.GetThis(NotConfigured);
            set
			{
                Configuration.SetThis(value);
                ZoneId = value;
				RequiresRebuildOfOut = true;
			}
		}

        /// <summary>
        /// This is a very internal setting, not to be used publicly for now.
        /// It will cause the App to not just return its data, but also data from its ancestors.
        /// EG global data.
        /// We're still evaluating impact on performance, confusion of developers etc.
        /// </summary>
        /// <remarks>
        /// Added in v15.04
        /// </remarks>
        [PrivateApi("WIP and not sure if this should ever become public")]
        [Configuration(Fallback = false)]
        public bool WithAncestors
        {
            get => Configuration.GetThis(false);
            set
            {
                Configuration.SetThis(value);
                RequiresRebuildOfOut = true;
            }
        }

        #endregion

        #region Constructor / DI


		public new class MyServices: MyServicesBase<DataSource.MyServices>
        {
            public DataSourceFactory DataSourceFactory { get; }
            public IAppStates AppStates { get; }

            public MyServices(DataSource.MyServices parentServices,
                IAppStates appStates,
				DataSourceFactory dataSourceFactory) : base(parentServices)
            {
                ConnectServices(
                    AppStates = appStates,
                    DataSourceFactory = dataSourceFactory
                );
            }
        }

		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		[PrivateApi]
		public App(MyServices services): base(services, $"{DataSourceConstants.LogPrefix}.EavApp")
        {
            _services = services;
            // this one is unusual, so don't pre-attach a default data stream to out
            _out = new StreamDictionary(this, null);
        }

        private readonly MyServices _services;

        #endregion


        /// <summary>
        /// Attach a different data source than is currently attached...
        /// this is needed when a zone/app change
        /// </summary>
        private void AttachOtherDataSource()
        {
            // If something is done badly, we can easily get recursions
            if (_attachOtherDataSourceRunning) throw new Exception("We have an unexpected recursion!");
            _attachOtherDataSourceRunning = true;
			// If we have zone/app switch, set not (they don't get updated if only the config is modified)
			// All not-set properties will use defaults 
			if (ZoneSwitch != 0) ZoneId = ZoneSwitch;
		    if (AppSwitch != 0) AppId = AppSwitch;

		    IDataSource appDs;

            
            // WIP / new
            if (WithAncestors)
            {
                Log.A("Will use Ancestors accessor with all ancestors");
                // Important: only pass the identity in, never pass this source in, or you'll get infinite recursions
                var appStack = _services.DataSourceFactory.Create<AppWithParents>(appIdentity: new AppIdentity(this), configLookUp: Configuration.LookUpEngine);
                appStack.AppId = AppId;
                appStack.ZoneId = ZoneId;
                appStack.ShowDrafts = ShowDrafts;
                appDs = appStack;
            }
            else
                appDs = _services.DataSourceFactory.GetPublishing(this,
                    configLookUp: Configuration.LookUpEngine, showDrafts: ShowDrafts);

            Attach(DataSourceConstants.DefaultStreamName, appDs);
            _attachOtherDataSourceRunning = false;
        }

        private bool _attachOtherDataSourceRunning = false;

		[PrivateApi]
		[Obsolete("Will probably be removed in v14")]
		// TODO: cause obsolete warning when used! #Deprecated
        public IMetadataSource Metadata => AppState;

		protected AppState AppState => _appState.Get(() => _services.AppStates.Get(this));
        private readonly GetOnce<AppState> _appState = new GetOnce<AppState>();
    }

}