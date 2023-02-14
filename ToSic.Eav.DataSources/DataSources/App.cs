using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Metadata;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
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
        In = new []{Constants.DefaultStreamName},
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
		#endregion

        #region Constructor / DI


		public new class Dependencies: ServiceDependencies<DataSource.Dependencies>
        {
            public LazySvc<DataSourceFactory> DataSourceFactory { get; }
            public IAppStates AppStates { get; }

            public Dependencies(DataSource.Dependencies rootDependencies,
                IAppStates appStates,
				LazySvc<DataSourceFactory> dataSourceFactory) : base(rootDependencies)
            {
                AddToLogQueue(
                    AppStates = appStates,
                    DataSourceFactory = dataSourceFactory
                );
            }
        }

		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		[PrivateApi]
		public App(Dependencies dependencies): base(dependencies.RootDependencies, $"{DataSourceConstants.LogPrefix}.EavApp")
        {
            _deps = dependencies.SetLog(Log);
            // this one is unusual, so don't pre-attach a default data stream to out
            _out = new StreamDictionary(this, null);
        }

        private readonly Dependencies _deps;

        #endregion


        /// <summary>
        /// Attach a different data source than is currently attached...
        /// this is needed when a zone/app change
        /// </summary>
        private void AttachOtherDataSource()
		{
			// all not-set properties will auto-initialize
			if (ZoneSwitch != 0)
				ZoneId = ZoneSwitch;
		    if (AppSwitch != 0)
				AppId = AppSwitch;

		    var newDs = _deps.DataSourceFactory.Value.GetPublishing(this, configProvider: Configuration.LookUpEngine, showDrafts:GetShowDraftStatus());
            Attach(Constants.DefaultStreamName, newDs);
		}

		[PrivateApi]
		[Obsolete("Will probably be removed in v14")]
		// TODO: cause obsolete warning when used! #Deprecated
        public IMetadataSource Metadata => AppState;

		protected AppState AppState => _appState.Get(() => _deps.AppStates.Get(this));
        private readonly GetOnce<AppState> _appState = new GetOnce<AppState>();
    }

}