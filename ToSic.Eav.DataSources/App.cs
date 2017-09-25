using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Caches;
using static System.Int32;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Return all Entities from a specific App
	/// </summary>
	[PipelineDesigner]
	public class App : BaseDataSource
	{
		#region Configuration-properties
		private const string AppSwitchKey = "AppSwitch";
		private const string ZoneSwitchKey = "ZoneSwitch";

	    public override string LogId => "DS-App";

	    /// <summary>
        /// An alternate app to switch to
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
		/// An alternate zone to switch to
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

		private readonly IDictionary<string, IDataStream> _out = new Dictionary<string, IDataStream>(StringComparer.OrdinalIgnoreCase);
		private bool _requiresRebuildOfOut = true;



	    public override IDictionary<string, IDataStream> Out
		{
			get
			{
                EnsureConfigurationIsLoaded();
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

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		public App()
		{
			// this one is unusual, so don't pre-attach a default data stream
			//Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities));

			// Set default switch-keys to 0 = no switch
			Configuration.Add(AppSwitchKey, "[Settings:" + AppSwitchKey + "||0]");
			Configuration.Add(ZoneSwitchKey, "[Settings:" + ZoneSwitchKey + "||0]");

            CacheRelevantConfigurations = new[] { AppSwitchKey, ZoneSwitchKey };
            TempUsesDynamicOut = true;
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

		    var newDs = DataSource.GetInitialDataSource(ZoneId, AppId);
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
			var cache = (BaseCache)DataSource.GetCache(zoneId: ZoneId, appId: AppId);
			var listOfTypes = cache.GetContentTypes();
		    foreach (var contentType in listOfTypes)
		    {
		        var typeName = contentType.Value.Name;
		        if (typeName != Constants.DefaultStreamName && !typeName.StartsWith("@") && !_out.ContainsKey(typeName))
		        {
		            var ds = DataSource.GetDataSource<EntityTypeFilter>(ZoneId, AppId, upstreamDataSource, ConfigurationProvider, parentLog:Log);
		            ds.TypeName = contentType.Value.Name;

		            ds.Out[Constants.DefaultStreamName].AutoCaching = true; // enable auto-caching 

		            _out.Add(contentType.Value.Name, ds.Out[Constants.DefaultStreamName]);
		        }
		    }
		}

	    private IMetaDataSource _metadata;

	    public IMetaDataSource Metadata => _metadata ?? (_metadata = DataSource.GetMetaDataSource(ZoneId, AppId));
	}

}