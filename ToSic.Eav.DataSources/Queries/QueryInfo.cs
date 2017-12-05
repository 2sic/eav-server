using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources.Queries
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns the attributes of a content-type
    /// </summary>
    [VisualQuery(Type = DataSourceType.Source,
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        EnableConfig = true,
        ExpectsDataOfType = "4638668f-d506-4f5c-ae37-aa7fdbbb5540",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-QueryInfo")]

    public sealed class QueryInfo: BaseDataSource
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavAts";

        private const string QueryTypeKey = "Query";
        private const string QueryNameField = "QueryName";
	    private const string DefQuery = "not-configured"; // can't be blank, otherwise tokens fail
	    private const string QueryStreamsContentType = "EAV_Query_Stream";
	    
        // 2dm: this is for a later feature...
	    // ReSharper disable once UnusedMember.Local
        private const string QueryCtGuid = "11001010-251c-eafe-2792-00000000000!";



	    private enum StreamsType
	    {
	        Name
	    }

        /// <summary>
        /// The content-type name
        /// </summary>
        public string QueryName
        {
            get => Configuration[QueryTypeKey];
            set => Configuration[QueryTypeKey] = value;
        }
        
		#endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
		public QueryInfo()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetStreams));
			Out.Add("Attributes", new DataStream(this, Constants.DefaultStreamName, GetAttributes));
            Configuration.Add(QueryTypeKey, $"[Settings:{QueryNameField}||{DefQuery}]");

            CacheRelevantConfigurations = new[] {QueryTypeKey};
		}

	    private IEnumerable<IEntity> GetStreams()
	    {
            EnsureConfigurationIsLoaded();
            BuildQuery();

	        if (_query == null) return new List<IEntity>();

	        var list = _query?.Out.OrderBy(stream => stream.Key).Select(stream
	            => new Data.Entity(AppId, 0,
	                QueryStreamsContentType, new Dictionary<string, object>
	                {
	                    {StreamsType.Name.ToString(), stream.Key},
	                }, StreamsType.Name.ToString()));

	        return list;
	    }

	    private IEnumerable<IEntity> GetAttributes()
	    {
            EnsureConfigurationIsLoaded();
            BuildQuery();

	        var attribInfo = DataSource.GetDataSource<Attributes>(upstream: _query);
	        var x = attribInfo.ConfigurationProvider;

	        return attribInfo.List;
        }

	    private IDataSource _query;

	    private void BuildQuery()
	    {
	        if (_alreadyBuilt) return;
            
            // find id
	        var queries = DataPipeline.AllQueryItems(AppId, Log);
            // important, use "Name" and not get-best-title, as some queries may not be correctly typed, so missing title-info
	        var found = queries.FirstOrDefault(q => string.Equals(q.GetBestValue("Name").ToString(), QueryName, StringComparison.InvariantCultureIgnoreCase));
	        if (found != null)
	        {
	            var id = found.EntityId;
	            _query = new DataPipelineFactory(Log).GetDataSourceForTesting(AppId, id, false);
	        }
	        _alreadyBuilt = true;
	    }

	    private bool _alreadyBuilt;

	}
}