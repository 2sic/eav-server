using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.System.Types;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.System
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns infos about a query. <br/>
    /// For example, it says how many out-streams are available and what fields can be used on each stream. <br/>
    /// This is used in fields which let you pick a query, stream and field from that stream.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "DataSources",
        Icon = "present_to_all",
        Type = DataSourceType.Source,
        GlobalName = "ToSic.Eav.DataSources.System.QueryInfo, ToSic.Eav.DataSources",
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        ExpectsDataOfType = "4638668f-d506-4f5c-ae37-aa7fdbbb5540",
        HelpLink = "https://docs.2sxc.org/api/dot-net/ToSic.Eav.DataSources.System.QueryInfo.html")]

    public sealed class QueryInfo: DataSourceBase
	{
        public QueryBuilder QueryBuilder { get; }
        private readonly Lazy<QueryManager> _queryManagerLazy;
        private readonly Lazy<GlobalQueries> _globalQueriesLazy;
        private QueryManager QueryManager => _queryManager ?? (_queryManager = _queryManagerLazy.Value.Init(Log));
        private QueryManager _queryManager;

        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavQIn";

        private const string QueryKey = "Query";
        private const string QueryNameField = "QueryName";
	    private const string DefQuery = "not-configured"; // can't be blank, otherwise tokens fail
        private const string StreamKey = "Stream";
        private const string StreamField = "StreamName";
	    private const string QueryStreamsContentType = "EAV_Query_Stream";
	    
        // 2dm: this is for a later feature...
	    // ReSharper disable once UnusedMember.Local
        private const string QueryCtGuid = "11001010-251c-eafe-2792-00000000000!";


        /// <summary>
        /// The content-type name
        /// </summary>
        public string QueryName
        {
            get => Configuration[QueryKey];
            set => Configuration[QueryKey] = value;
        }
        
        public string StreamName
        {
            get => Configuration[StreamKey];
            set => Configuration[StreamKey] = value;
        }
		#endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
		public QueryInfo(Lazy<QueryManager> queryManagerLazy, QueryBuilder queryBuilder, Lazy<GlobalQueries> globalQueriesLazy)
		{
            QueryBuilder = queryBuilder.Init(Log);
            _queryManagerLazy = queryManagerLazy;
            _globalQueriesLazy = globalQueriesLazy;
            Provide(GetStreams);
			Provide("Attributes", GetAttributes);
		    ConfigMask(QueryKey, $"[Settings:{QueryNameField}||{DefQuery}]");
		    ConfigMask(StreamKey, $"[Settings:{StreamField}||{Constants.DefaultStreamName}]");
		}

	    private ImmutableArray<IEntity> GetStreams()
	    {
            CustomConfigurationParse();

            return _query?.Out.OrderBy(stream => stream.Key).Select(stream
                           => Build.Entity(new Dictionary<string, object>
                               {
                                   {StreamsType.Name.ToString(), stream.Key}
                               },
                               titleField: StreamsType.Name.ToString(),
                               typeName: QueryStreamsContentType))
                       .ToImmutableArray()
                   ?? ImmutableArray<IEntity>.Empty;// new List<IEntity>().ToImmutableList();
        }

	    private IImmutableList<IEntity> GetAttributes()
	    {
            CustomConfigurationParse();

            // no query can happen if the name was blank
            if (_query == null)
                return ImmutableArray<IEntity>.Empty;//  new List<IEntity>();

            // check that _query has the stream name
            if (!_query.Out.ContainsKey(StreamName))
                return ImmutableArray<IEntity>.Empty; //new List<IEntity>();

	        var attribInfo = DataSourceFactory.GetDataSource<Attributes>(_query);
            if(StreamName != Constants.DefaultStreamName)
                attribInfo.Attach(Constants.DefaultStreamName, _query[StreamName]);

	        return attribInfo.Immutable;
        }

	    private void CustomConfigurationParse()
	    {
            Configuration.Parse();
            BuildQuery();
	    }


        private void BuildQuery()
        {
            if (string.IsNullOrWhiteSpace(QueryName))
                return;

            // important, use "Name" and not get-best-title, as some queries may not be correctly typed, so missing title-info
            var found = QueryName.StartsWith(GlobalQueries.GlobalEavQueryPrefix)
                ? _globalQueriesLazy.Value.FindQuery(QueryName)
                : this.QueryManager.AllQueryItems(this)
                    .FirstOrDefault(q => string.Equals(q.Value<string>("Name"), QueryName,
                        StringComparison.InvariantCultureIgnoreCase));

            if (found == null) throw new Exception($"Can't build information about query - couldn't find query '{QueryName}'");

            _query = QueryBuilder.GetDataSourceForTesting(new QueryDefinition(found, AppId, Log), 
                false, Configuration.LookUpEngine);
        }

	    private IDataSource _query;

	}
}