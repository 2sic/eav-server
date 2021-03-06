﻿using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using static System.Int32;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// All the data inside an App. <br/>
	/// For example, it has a variable amount of Out-streams, one for each content-type in the app.
	/// </summary>

    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.App, ToSic.Eav.DataSources",
        Type = DataSourceType.Source, 
        Icon = "app",
        DynamicOut = true,
		NiceName = "App",
		UiHint = "with streams for each Content Type",
		ExpectsDataOfType = "|Config ToSic.Eav.DataSources.App",
        HelpLink = "https://r.2sxc.org/DsApp")]
    public partial class App : DataSourceBase
	{
		#region Configuration-properties
		private const string AppSwitchKey = "AppSwitch";
		private const string ZoneSwitchKey = "ZoneSwitch";

        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.EavApp";

	    /// <summary>
        /// Use this to re-target the app-source to another app. <br/>
        /// Note that this can only be done before ever accessing the app - once the object has started reading data, switching has no more effect.
        /// </summary>
        public int AppSwitch
		{
			get => Parse(Configuration[AppSwitchKey]);
		    set
			{
				Configuration[AppSwitchKey] = value.ToString();
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
			get => Parse(Configuration[ZoneSwitchKey]);
		    set
			{
				Configuration[ZoneSwitchKey] = value.ToString();
				ZoneId = value;
				RequiresRebuildOfOut = true;
			}
		}
		#endregion


		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		[PrivateApi]
		public App()
		{
			// this one is unusual, so don't pre-attach a default data stream to out
            _out = new StreamDictionary(this, null);

			// Set default switch-keys to 0 = no switch
			ConfigMask(AppSwitchKey, "[Settings:" + AppSwitchKey + "||0]");
			ConfigMask(ZoneSwitchKey, "[Settings:" + ZoneSwitchKey + "||0]");

            OutIsDynamic = true;
        }

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
		    if (In.ContainsKey(Constants.DefaultStreamName))
		        In.Remove(Constants.DefaultStreamName);
			In.Add(Constants.DefaultStreamName, newDs[Constants.DefaultStreamName]);
		}


		/// <summary>
		/// Metadata is an important feature of apps. <br/>
		/// The App DataSource automatically provides direct access to the metadata system.
		/// This allows users of the App to query metadata directly through this object. 
		/// </summary>
		/// <returns>An initialized <see cref="IMetadataSource"/> for this app</returns>
		public IMetadataSource Metadata => _metadata ?? (_metadata = Apps.State.Get(this));
        private IMetadataSource _metadata;
    }

}