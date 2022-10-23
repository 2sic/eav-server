using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;
using static System.Int32;

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
    public partial class App : DataSourceBase
	{
        #region Configuration-properties

        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.EavApp";

	    /// <summary>
        /// Use this to re-target the app-source to another app. <br/>
        /// Note that this can only be done before ever accessing the app - once the object has started reading data, switching has no more effect.
        /// </summary>
        public int AppSwitch
		{
			get => Parse(Configuration[nameof(AppSwitch)]);
		    set
			{
				Configuration[nameof(AppSwitch)] = value.ToString();
				AppId = value;
				RequiresRebuildOfOut = true;
			}
		}

        /// <summary>
        /// Use this to re-target the app-source to another zone. <br/>
        /// Note that this can only be done before ever accessing the app - once the object has started reading data, switching has no more effect.
        /// </summary>
		public int ZoneSwitch
		{
			get => Parse(Configuration[nameof(ZoneSwitch)]);
		    set
			{
				Configuration[nameof(ZoneSwitch)] = value.ToString();
				ZoneId = value;
				RequiresRebuildOfOut = true;
			}
		}
		#endregion


		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		[PrivateApi]
		public App(IAppStates appStates)
		{
            _appStates = appStates;
            // this one is unusual, so don't pre-attach a default data stream to out
            _out = new StreamDictionary(this, null);

			// Set default switch-keys to 0 = no switch
            ConfigMask(nameof(AppSwitch) + "||0");
			ConfigMask(nameof(ZoneSwitch) + "||0");
        }
        private readonly IAppStates _appStates;

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

		    var newDs = DataSourceFactory.GetPublishing(this, configProvider: Configuration.LookUpEngine, showDrafts:GetShowDraftStatus());
            Attach(Constants.DefaultStreamName, newDs);
		}

		[PrivateApi]
		[Obsolete("Will probably be removed in v14")]
		// TODO: cause obsolete warning when used! #Deprecated
        public IMetadataSource Metadata => AppState;

		protected AppState AppState => _appState ?? (_appState = _appStates.Get(this));
        private AppState _appState;
    }

}