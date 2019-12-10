using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Provides a data-source to a query, but won't assemble/compile the query unless accessed (lazy). 
	/// </summary>
	[PublicApi]
	public sealed class Query : DataSourceBase, IQuery
	{
        #region Configuration-properties
        [PrivateApi]
	    public override string LogId => "DS.DefQry";

        /// <inheritdoc />
        public QueryDefinition Definition { get; }

		private IDictionary<string, IDataStream> _out = new Dictionary<string, IDataStream>();
		private bool _requiresRebuildOfOut = true;
        private readonly bool _showDrafts;

        /// <summary>
        /// Standard out. Note that the Out is not prepared until accessed the first time,
        /// when it will auto-assembles the query
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
		[PrivateApi]
		public Query(int zoneId, int appId, IEntity queryDef, ILookUpEngine config, bool showDrafts)
		{
		    ZoneId = zoneId;
		    AppId = appId;
            Definition = new QueryDefinition(queryDef, appId, Log);
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
		    var pipeline = QueryBuilder.GetAsDataSource(Definition, ConfigurationProvider, 
                null, null, _showDrafts);
		    _out = pipeline.Out;
        }

        private QueryBuilder QueryBuilder => _queryBuilder ?? (_queryBuilder = new QueryBuilder(Log));
        private QueryBuilder _queryBuilder;



        /// <inheritdoc />
        public void Param(string key, string value)
        {
            // if the query has already been built, and we're changing a value, make sure we'll regenerate the results
            if(!_requiresRebuildOfOut)
                throw new Exception("Can't set param any more, the query has already been compiled. Always set params before accessing the data. To Re-Run the query with other params, call Reset() first.");
            //if (!Definition.Params.ContainsKey(value) || Definition.Params[key] != value)
            //{
            //    Log.Add($"Will set another param {key} to '{value}' - rebuild will be necessary on next read");
            //    _requiresRebuildOfOut = true;
            //}
            Definition.Params[key] = value;
        }

        /// <inheritdoc />
        public void Reset()
        {
            Definition.Reset();
            _requiresRebuildOfOut = true;
        }
    }

}