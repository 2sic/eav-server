using System.Collections.Generic;
using ToSic.Eav.DataSources.Query;
using ToSic.Eav.LookUp;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Provides a data-source to a query, but won't assemble the query unless accessed
	/// </summary>
	public sealed class DeferredQuery : BaseDataSource
	{
        #region Configuration-properties
	    public override string LogId => "DS.DefQry";

        public IEntity QueryDefinition;

		private IDictionary<string, IDataStream> _out = new Dictionary<string, IDataStream>();
		private bool _requiresRebuildOfOut = true;
        private bool _showDrafts;

        /// <summary>
        /// Ensures that the Out doesn't need assembling till accessed, and then auto-assembles it all
        /// </summary>
		public override IDictionary<string, IDataStream> Out
		{
			get
			{
			    if (!_requiresRebuildOfOut) return _out;
			    CreateOutWithAllStreams();
			    _requiresRebuildOfOut = false;
			    return _out;
			}
		}
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		public DeferredQuery(int zoneId, int appId, IEntity queryDef, ILookUpEngine config, bool showDrafts)
		{
		    ZoneId = zoneId;
		    AppId = appId;
		    QueryDefinition = queryDef;
		    ConfigurationProvider = config;
            _showDrafts = showDrafts;

		    // this one is unusual, so don't pre-attach a default data stream
		    //Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities));
		}

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
		{
		    var pipeln = new QueryBuilder(Log).GetAsDataSource(AppId, QueryDefinition, ConfigurationProvider, null, _showDrafts);
		    _out = pipeln.Out;
		}
	}

}