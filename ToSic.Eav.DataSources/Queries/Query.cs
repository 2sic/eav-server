﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Provides a data-source to a query, but won't assemble/compile the query unless accessed (lazy). 
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public sealed class Query : DataSourceBase, IQuery
	{
        #region Configuration-properties
        [PrivateApi]
	    public override string LogId => $"{DataSourceConstants.LogPrefix}.Query";

        /// <inheritdoc />
        public QueryDefinition Definition { get; private set; }

		private StreamDictionary _out = new StreamDictionary();
		private bool _requiresRebuildOfOut = true;
        private bool _showDrafts;

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

        #region Internal Source - mainly for debugging or advanced uses of a query

        [PrivateApi]
        public IDataSource Source
        {
            get
            {
                if (!_requiresRebuildOfOut) return _source;
                CreateOutWithAllStreams();
                _requiresRebuildOfOut = false;
                return _source;
            }
        }

        private IDataSource _source;
        #endregion

        /// <inheritdoc />
        [PrivateApi]
        public Query(DataSourceFactory dataSourceFactory)
        {
            DataSourceFactory = dataSourceFactory.Init(Log);
        }

        /// <summary>
        /// Initialize a full query object. This is necessary for it to work
        /// </summary>
        /// <returns></returns>
        [PrivateApi]
		public Query Init(int zoneId, int appId, IEntity queryDef, ILookUpEngine config, bool showDrafts, IDataTarget source, ILog parentLog)
		{
		    ZoneId = zoneId;
		    AppId = appId;
            Log.LinkTo(parentLog, LogId);
            Definition = new QueryDefinition(queryDef, appId, Log);
            this.Init(config);
            //Configuration.LookUps = config;
            _showDrafts = showDrafts;

            // hook up in, just in case we get parameters from an In
            if (source == null) return this;

            Log.Add("found target for Query, will attach");
            In = source.In;
            return this;
        }

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
        {
            var wrapLog = Log.Call(message:$"Query: '{Definition.Entity.GetBestTitle()}'", useTimer: true);

            // Step 1: Resolve the params from outside, where x=[Params:y] should come from the outer Params
            // and the current In
            var resolvedParams = Configuration.LookUpEngine.LookUp(Definition.Params);

            // now provide an override source for this
            var paramsOverride = new LookUpInDictionary(QueryConstants.ParamsLookup, resolvedParams);
		    _source = QueryBuilder.BuildQuery(Definition, Configuration.LookUpEngine, 
                new List<ILookUp> {paramsOverride}, _showDrafts);
            _out = new StreamDictionary(this, _source.Out);
            wrapLog("ok");
        }

        private QueryBuilder QueryBuilder => _queryBuilder ?? (_queryBuilder = DataSourceFactory.ServiceProvider.Build<QueryBuilder>().Init(Log));
        private QueryBuilder _queryBuilder;



        /// <inheritdoc />
        public void Params(string key, string value)
        {
            var wrapLog = Log.Call($"{key}, {value}");
            // if the query has already been built, and we're changing a value, make sure we'll regenerate the results
            if(!_requiresRebuildOfOut)
            {
                Log.Add("Can't set param - query already compiled");
                wrapLog("error");
                throw new Exception("Can't set param any more, the query has already been compiled. " +
                                    "Always set params before accessing the data. " +
                                    "To Re-Run the query with other params, call Reset() first.");
            }

            Definition.Params[key] = value;
            wrapLog(null);
        }


        /// <inheritdoc />
        public void Params(string list) => Params(QueryDefinition.GenerateParamsDic(list, Log));

        /// <inheritdoc />
        public void Params(IDictionary<string, string> values)
        {
            foreach (var qP in values)
                Params(qP.Key, qP.Value);
        }

        /// <inheritdoc />
        public IDictionary<string, string> Params() => Definition.Params;

        /// <inheritdoc />
        public void Reset()
        {
            Log.Add("Reset query");
            Definition.Reset();
            _requiresRebuildOfOut = true;
        }

        /// <summary>
        /// Override PurgeList, because we don't really have In streams, unless we use parameters. 
        /// </summary>
        /// <param name="cascade"></param>
        public override void PurgeList(bool cascade = false)
        {
            var callLog = Log.Call($"{cascade}", $"on {GetType().Name}");
            // PurgeList on all In, as would usually happen
            // This will only purge query-in used for parameter
            base.PurgeList(cascade);

            Log.Add("Now purge the lists which the Query has on the Out");
            foreach (var stream in Source.Out)
                stream.Value.PurgeList(cascade);
            if (!Source.Out.Any()) Log.Add("No streams on Source.Out found to clear");

            Log.Add("Update RequiresRebuildOfOut");
            _requiresRebuildOfOut = true;
            callLog("ok");

        }
    }

}