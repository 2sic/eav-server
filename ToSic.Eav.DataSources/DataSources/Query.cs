using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
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
		    var pipeline = new QueryBuilder(Log).GetAsDataSource(Definition, ConfigurationProvider, 
                null, null, _showDrafts);
		    _out = pipeline.Out;
        }


        /// <inheritdoc />
        public void Param(string key, string value)
        {
            if(!_requiresRebuildOfOut)
                throw new Exception("Can't set param any more, the query has already been compiled. Always set params before accessing the data.");
            Definition.Params[key] = value;
        }
    }

}