using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using static System.String;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Provides a data-source to a query, but won't assemble/compile the query unless accessed (lazy). 
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public sealed class Query : DataSource, IQuery
	{
        #region Configuration-properties

        /// <inheritdoc />
        public QueryDefinition Definition { get; private set; }

		private StreamDictionary _out = new StreamDictionary();
		private bool _requiresRebuildOfOut = true;

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
        public Query(MyServices services, LazySvc<QueryBuilder> queryBuilder) : base(services, $"{DataSourceConstants.LogPrefix}.Query")
        {
            ConnectServices(
                _queryBuilderLazy = queryBuilder
            );
        }
        private readonly LazySvc<QueryBuilder> _queryBuilderLazy;

        /// <summary>
        /// Initialize a full query object. This is necessary for it to work
        /// </summary>
        /// <returns></returns>
        [PrivateApi]
		public Query Init(int zoneId, int appId, IEntity queryDef, ILookUpEngine configSource, IDataSourceTarget source = null)
		{
            ZoneId = zoneId;
		    AppId = appId;
            Definition = QueryBuilder.Create(queryDef, appId);
            this.Init(configSource);

            // hook up in, just in case we get parameters from an In
            if (source == null) return this;

            Log.A("found target for Query, will attach");
            In = source.In;
            return this;
        }

        /// <summary>
        /// Create a stream for each data-type
        /// </summary>
        private void CreateOutWithAllStreams() => Log.Do(timer: true, message: $"Query: '{Definition.Entity.GetBestTitle()}'", action: () =>
        {
            // Step 1: Resolve the params from outside, where x=[Params:y] should come from the outer Params
            // and the current In
            var resolvedParams = Configuration.LookUpEngine.LookUp(Definition.Params);

            // now provide an override source for this
            var paramsOverride = new LookUpInDictionary(QueryConstants.ParamsSourceName, resolvedParams);
            var queryInfos = QueryBuilder.BuildQuery(Definition, Configuration.LookUpEngine,
                new List<ILookUp> { paramsOverride });
            _source = queryInfos.Item1;
            _out = new StreamDictionary(this, _source.Out);
        });

        private QueryBuilder QueryBuilder => _queryBuilder.Get(() => _queryBuilderLazy.Value);
        private readonly GetOnce<QueryBuilder> _queryBuilder = new GetOnce<QueryBuilder>();



        /// <inheritdoc />
        public void Params(string key, string value) => Log.Do($"{key}, {value}", l =>
        {
            if (IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            // if the query has already been built, and we're changing a value, make sure we'll regenerate the results
            if (!_requiresRebuildOfOut)
            {
                l.A("Can't set param - query already compiled, error");
                throw new Exception("Can't set param any more, the query has already been compiled. " +
                                    "Always set params before accessing the data. " +
                                    "To Re-Run the query with other params, call Reset() first.");
            }

            Definition.Params[key] = value;
        });

        /// <inheritdoc />
        public void Params(string key, object value) => Params(key, value?.ToString());


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
            Log.A("Reset query");
            Definition.Reset();
            _requiresRebuildOfOut = true;
        }

        /// <summary>
        /// Override PurgeList, because we don't really have In streams, unless we use parameters. 
        /// </summary>
        /// <param name="cascade"></param>
        public override void PurgeList(bool cascade = false) => Log.Do($"{cascade} - on {GetType().Name}", l =>
        {
            // PurgeList on all In, as would usually happen
            // This will only purge query-in used for parameter
            base.PurgeList(cascade);

            l.A("Now purge the lists which the Query has on the Out");
            foreach (var stream in Source.Out)
                stream.Value.PurgeList(cascade);
            if (!Source.Out.Any()) l.A("No streams on Source.Out found to clear");

            l.A("Update RequiresRebuildOfOut");
            _requiresRebuildOfOut = true;

        });
    }

}