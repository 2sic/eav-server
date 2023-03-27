using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Eav.Services;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Sys
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns infos about a query. <br/>
    /// For example, it says how many out-streams are available and what fields can be used on each stream. <br/>
    /// This is used in fields which let you pick a query, stream and field from that stream.
    /// </summary>
    /// <remarks>
    /// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "DataSources",
        UiHint = "List the DataSources available in the system",
        Icon = Icons.ArrowUpBoxed,
        Type = DataSourceType.System,
        NameId = "ToSic.Eav.DataSources.System.QueryInfo, ToSic.Eav.DataSources",
        Audience = Audience.Advanced,
        DynamicOut = false,
        ConfigurationType = "4638668f-d506-4f5c-ae37-aa7fdbbb5540",
        HelpLink = "https://docs.2sxc.org/api/dot-net/ToSic.Eav.DataSources.System.QueryInfo.html")]

    public sealed class QueryInfo : Eav.DataSource.DataSourceBase
    {
        private readonly IDataSourceGenerator<Attributes> _attributesGenerator;
        private readonly IDataFactory _dataFactory;
        public QueryBuilder QueryBuilder { get; }
        private readonly LazySvc<QueryManager> _queryManagerLazy;
        private QueryManager QueryManager => _queryManager ?? (_queryManager = _queryManagerLazy.Value);
        private QueryManager _queryManager;

        #region Configuration-properties (no config)

        private const string DefQuery = "not-configured"; // can't be blank, otherwise tokens fail
        private const string QueryStreamsContentType = "QueryStream";

        /// <summary>
        /// The content-type name
        /// </summary>
        [Configuration(Fallback = DefQuery)]
        public string QueryName => Configuration.GetThis();

        [Configuration(Fallback = StreamDefaultName)]
        public string StreamName => Configuration.GetThis();

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
        public QueryInfo(MyServices services,
            LazySvc<QueryManager> queryManagerLazy, QueryBuilder queryBuilder, IDataFactory dataFactory, IDataSourceGenerator<Attributes> attributesGenerator) : base(
            services, $"{LogPrefix}.EavQIn")
        {
            ConnectServices(
                QueryBuilder = queryBuilder,
                _queryManagerLazy = queryManagerLazy,
                _attributesGenerator = attributesGenerator,
                _dataFactory = dataFactory.New(options: new DataFactoryOptions(typeName: QueryStreamsContentType, titleField: StreamsType.Name.ToString()))
            );
            ProvideOut(GetStreams);
            ProvideOut(GetAttributes, "Attributes");
        }

        private IImmutableList<IEntity> GetStreams() => Log.Func(l =>
        {
            CustomConfigurationParse();

            var result = _query?.Out.OrderBy(stream => stream.Key).Select(stream
                                 => _dataFactory.Create(new Dictionary<string, object>
                                 {
                                     {StreamsType.Name.ToString(), stream.Key}
                                 }))
                             .ToImmutableList()
                         ?? EmptyList;
            return (result, $"{result.Count}");
        });

        private IImmutableList<IEntity> GetAttributes() => Log.Func(l => 
        {
            CustomConfigurationParse();

            // no query can happen if the name was blank
            if (_query == null)
                return (EmptyList, "null");

            // check that _query has the stream name
            if (!_query.Out.ContainsKey(StreamName))
                return (EmptyList, "can't find stream name in query");

            var attribInfo = _attributesGenerator.New(attach: _query);
            if (StreamName != StreamDefaultName)
                attribInfo.Attach(StreamDefaultName, _query, StreamName);

            var results = attribInfo.List.ToImmutableList();
            return (results, $"{results.Count}");
        });

        private void CustomConfigurationParse()
        {
            Configuration.Parse();
            BuildQuery();
        }


        private void BuildQuery() => Log.Do(() =>
        {
            var qName = QueryName;
            if (string.IsNullOrWhiteSpace(qName))
                return;

            // important, use "Name" and not get-best-title, as some queries may not be correctly typed, so missing title-info
            var found = QueryManager.FindQuery(this, qName, recurseParents: 3);
                //qName.StartsWith(DataSourceConstants.GlobalEavQueryPrefix)
                //? QueryManager.FindQuery(Constants.PresetIdentity, qName)
                //: QueryManager.FindQuery(this, qName);

            if (found == null)
                throw new Exception($"Can't build information about query - couldn't find query '{qName}'");

            var builtQuery = QueryBuilder.GetDataSourceForTesting(QueryBuilder.Create(found, AppId),
                lookUps: Configuration.LookUpEngine);
            _query = builtQuery.Main;
        });

        private IDataSource _query;

    }
}