using System.Collections.Generic;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.Interfaces;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Provides a data-source to a query (pipeline), but won't assemble the pipeline unless accessed
	/// </summary>
	public class DeferredPipelineQuery : BaseDataSource
	{
        #region Configuration-properties
	    public override string LogId => "DS.DefQry";

        public IEntity QueryDefinition;

		private IDictionary<string, IDataStream> _Out = new Dictionary<string, IDataStream>();
		private bool _requiresRebuildOfOut = true;

        /// <summary>
        /// Ensures that the Out doesn't need assembling till accessed, and then auto-assembles it all
        /// </summary>
		public override IDictionary<string, IDataStream> Out
		{
			get
			{
				if (_requiresRebuildOfOut)
				{
				    CreateOutWithAllStreams();
					_requiresRebuildOfOut = false;
				}
				return _Out;
			}
		}
		#endregion

		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		public DeferredPipelineQuery(int zoneId, int appId, IEntity queryDef, IValueCollectionProvider config)
		{
		    ZoneId = zoneId;
		    AppId = appId;
		    QueryDefinition = queryDef;
		    ConfigurationProvider = config;

		    // this one is unusual, so don't pre-attach a default data stream
		    //Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities));
		}

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
		{
		    var pipeln = new DataPipelineFactory(Log).GetDataSource(AppId, QueryDefinition/*.EntityId*/, ConfigurationProvider as ValueCollectionProvider);
		    _Out = pipeln.Out;
		}
	}

}