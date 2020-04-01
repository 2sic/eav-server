using System;
using System.Collections.Generic;
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
            CustomConfigurationParse();

            return _query?.Out.OrderBy(stream => stream.Key).Select(stream
                => Build.Entity(new Dictionary<string, object>
                           {
                               {StreamsType.Name.ToString(), stream.Key}
                           }, 
                    titleField: StreamsType.Name.ToString(), 
                    typeName:QueryStreamsContentType)
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
            CustomConfigurationParse();

            // no query can happen if the name was blank
            if (_query == null)
                return new List<IEntity>();

            // check that _query has the stream name
            if(!_query.Out.ContainsKey(StreamName))
                return new List<IEntity>();

	        var attribInfo = new DataSource(Log).GetDataSource<Attributes>(_query);
            if(StreamName != Constants.DefaultStreamName)
                attribInfo.Attach(Constants.DefaultStreamName, _query[StreamName]);

	        return attribInfo.List;
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
                ? GlobalQueries.FindQuery(QueryName)
                : QueryManager.AllQueryItems(/*AppId*/this, Log)
                    .FirstOrDefault(q => string.Equals(q.GetBestValue("Name").ToString(), QueryName,
                        StringComparison.InvariantCultureIgnoreCase));

            if (found == null) throw new Exception($"Can't build information about query - couldn't find query '{QueryName}'");

            _query = new QueryBuilder(Log).GetDataSourceForTesting(new QueryDefinition(found, AppId, Log), 
                false, Configuration.LookUps);
        }

	    private IDataSource _query;

	}
}