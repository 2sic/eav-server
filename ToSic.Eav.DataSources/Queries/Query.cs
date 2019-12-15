﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
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
	    public override string LogId => "DS.Query";

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
		public Query(int zoneId, int appId, IEntity queryDef, ILookUpEngine config, bool showDrafts, ILog parentLog)
		{
		    ZoneId = zoneId;
		    AppId = appId;
            Definition = new QueryDefinition(queryDef, appId, Log);
            Configuration.LookUps = config;
            _showDrafts = showDrafts;
            Log.LinkTo(parentLog, LogId);
        }

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
        {
            var wrapLog = Log.Call();
		    var pipeline = QueryBuilder.GetAsDataSource(Definition, Configuration.LookUps, 
                null, null, _showDrafts);
		    _out = pipeline.Out;
            wrapLog("ok");
        }

        private QueryBuilder QueryBuilder => _queryBuilder ?? (_queryBuilder = new QueryBuilder(Log));
        private QueryBuilder _queryBuilder;



        /// <inheritdoc />
        public void Params(string key, string value)
        {
            // if the query has already been built, and we're changing a value, make sure we'll regenerate the results
            if(!_requiresRebuildOfOut)
                throw new Exception("Can't set param any more, the query has already been compiled. " +
                                    "Always set params before accessing the data. " +
                                    "To Re-Run the query with other params, call Reset() first.");

            Definition.Params[key] = value;
        }


        /// <inheritdoc />
        public void Params(string list)
        {
            foreach (var qP in QueryDefinition.GenerateParamsDic(list, Log)) 
                Params(qP.Key, qP.Value);
        }

        /// <inheritdoc />
        public IDictionary<string, string> Params() => Definition.Params;

        /// <inheritdoc />
        public void Reset()
        {
            Definition.Reset();
            _requiresRebuildOfOut = true;
        }
    }

}