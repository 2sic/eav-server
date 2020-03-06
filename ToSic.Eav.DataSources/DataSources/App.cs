using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Queries;
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
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.App",
        HelpLink = "https://r.2sxc.org/DsApp")]
    public class App : DataSourceBase
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
				_requiresRebuildOfOut = true;
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
				_requiresRebuildOfOut = true;
			}
		}
		#endregion
		#region Dynamic Out
		private readonly StreamDictionary _out; // Dictionary<string, IDataStream>(StringComparer.OrdinalIgnoreCase);
		private bool _requiresRebuildOfOut = true;



        /// <inheritdoc/>
	    public override IDictionary<string, IDataStream> Out
		{
			get
			{
                Configuration.Parse();
                if (_requiresRebuildOfOut)
				{
					// if the rebuilt is required because the app or zone are not default, then attach it first
					if (AppSwitch != 0 || ZoneSwitch != 0)
						AttachOtherDataSource();
					// now create all streams
					CreateOutWithAllStreams();
					_requiresRebuildOfOut = false;
				}
				return _out;
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

		    var newDs = new DataSource(Log).GetPublishing(this, configProvider: Configuration.LookUps);
		    if (In.ContainsKey(Constants.DefaultStreamName))
		        In.Remove(Constants.DefaultStreamName);
			In.Add(Constants.DefaultStreamName, newDs[Constants.DefaultStreamName]);
		}

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
		{
			IDataStream upstream;
			try
			{
                // auto-attach to cache of current system?
                if(!In.ContainsKey(Constants.DefaultStreamName))
                    AttachOtherDataSource();
				upstream = In[Constants.DefaultStreamName];
			}
			catch (KeyNotFoundException)
			{
                throw new Exception("Trouble with the App DataSource - must have a Default In-Stream with name " + Constants.DefaultStreamName + ". It has " + In.Count + " In-Streams.");
			}

			var upstreamDataSource = upstream.Source;
			_out.Clear();
			_out.Add(Constants.DefaultStreamName, upstreamDataSource.Out[Constants.DefaultStreamName]);

			// now provide all data streams for all data types; only need the cache for the content-types list, don't use it as the source...
			// because the "real" source already applies filters like published
            var listOfTypes = Apps.State.Get(this).ContentTypes;
            var dataSourceFactory = new DataSource(Log);
		    foreach (var contentType in listOfTypes)
		    {
		        var typeName = contentType.Name;
                if (typeName == Constants.DefaultStreamName || typeName.StartsWith("@") ||
                    _out.ContainsKey(typeName)) continue;
                var deferredStream = new DataStreamDeferred(this, typeName, 
                    () => BuildTypeStream(dataSourceFactory, upstreamDataSource, typeName)[Constants.DefaultStreamName], true);
                _out.Add(typeName, deferredStream);
            }
		}

		/// <summary>
		/// Build an EntityTypeFilter for this content-type to provide as a stream
		/// </summary>
        private EntityTypeFilter BuildTypeStream(DataSource dataSourceFactory, IDataSource upstreamDataSource, string typeName)
        {
            var ds = dataSourceFactory.GetDataSource<EntityTypeFilter>(this, upstreamDataSource,
                Configuration.LookUps);
            ds.TypeName = typeName;
            ds.Guid = Guid; // tell the inner source that it has the same ID as this one, as we're pretending it's the same source
            ds.Out[Constants.DefaultStreamName].AutoCaching = true; // enable auto-caching 
            return ds;
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