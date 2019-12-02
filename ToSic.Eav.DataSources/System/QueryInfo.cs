using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.System.Types;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.System
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns the attributes of a content-type
    /// </summary>
    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.System.QueryInfo, ToSic.Eav.DataSources",
        Type = DataSourceType.Source,
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        ExpectsDataOfType = "4638668f-d506-4f5c-ae37-aa7fdbbb5540",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-QueryInfo")]

    public sealed class QueryInfo: DataSourceBase
	{
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
		public QueryInfo()
		{
			Provide(GetStreams);
			Provide("Attributes", GetAttributes);
		    ConfigMask(QueryKey, $"[Settings:{QueryNameField}||{DefQuery}]");
		    ConfigMask(StreamKey, $"[Settings:{StreamField}||{Constants.DefaultStreamName}]");
		}

	    private IEnumerable<IEntity> GetStreams()
	    {
	        EnsureConfigurationIsLoaded();

	        return _query?.Out.OrderBy(stream => stream.Key).Select(stream
                => AsEntity(new Dictionary<string, object>
                           {
                               {StreamsType.Name.ToString(), stream.Key}
                           }, StreamsType.Name.ToString(), QueryStreamsContentType)
                       //=> new Data.Entity(AppId, 0, QueryStreamsContentType,
                       //    new Dictionary<string, object>
                       //    {
                       //        {StreamsType.Name.ToString(), stream.Key}
                       //    },
                       //    StreamsType.Name.ToString())
                       )
	               ?? new List<IEntity>();
	    }

	    private IEnumerable<IEntity> GetAttributes()
	    {
            EnsureConfigurationIsLoaded();

            // no query can happen if the name was blank
            if(_query == null)
                return new List<IEntity>();

            // check that _query has the stream name
            if(!_query.Out.ContainsKey(StreamName))
                return new List<IEntity>();

	        var attribInfo = DataSource.GetDataSource<Attributes>(upstream: _query, configLookUp:_query.ConfigurationProvider);
            if(StreamName != Constants.DefaultStreamName)
                attribInfo.Attach(Constants.DefaultStreamName, _query[StreamName]);

	        return attribInfo.List;
        }

	    protected internal override void EnsureConfigurationIsLoaded()
	    {
	        base.EnsureConfigurationIsLoaded();
            BuildQuery();
	    }


        private void BuildQuery()
        {
            if (string.IsNullOrWhiteSpace(QueryName))
                return;

            // important, use "Name" and not get-best-title, as some queries may not be correctly typed, so missing title-info
            var found = QueryName.StartsWith(GlobalQueries.GlobalQueryPrefix)
                ? GlobalQueries.FindQuery(QueryName)
                : QueryManager.AllQueryItems(AppId, Log)
                    .FirstOrDefault(q => string.Equals(q.GetBestValue("Name").ToString(), QueryName,
                        StringComparison.InvariantCultureIgnoreCase));

            if (found == null) throw new Exception($"Can't build information about query - couldn't find query '{QueryName}'");

            _query = new QueryBuilder(Log).GetDataSourceForTesting(new QueryDefinition(found, AppId), false, ConfigurationProvider);
        }

	    private IDataSource _query;

	}
}