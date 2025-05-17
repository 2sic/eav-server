using ToSic.Eav.DataSource.Internal.Query;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Services;
using ToSic.Lib.Helpers;
using ToSic.Lib.Internal.Generics;
using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Sys;

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
    Icon = DataSourceIcons.ArrowUpBoxed,
    Type = DataSourceType.System,
    NameId = "ToSic.Eav.DataSources.System.QueryInfo, ToSic.Eav.DataSources",
    Audience = Audience.Advanced,
    DynamicOut = false,
    ConfigurationType = "4638668f-d506-4f5c-ae37-aa7fdbbb5540",
    HelpLink = "https://docs.2sxc.org/api/dot-net/ToSic.Eav.DataSources.System.QueryInfo.html")]

public sealed class QueryInfo : CustomDataSourceAdvanced
{
    private readonly IDataSourceGenerator<Attributes> _attributesGenerator;
    public QueryBuilder QueryBuilder { get; }
    private readonly LazySvc<QueryManager> _queryManager;

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
        LazySvc<QueryManager> queryManager, QueryBuilder queryBuilder, IDataSourceGenerator<Attributes> attributesGenerator) : base(
        services, $"{DataSourceConstantsInternal.LogPrefix}.EavQIn", connect: [queryBuilder, queryManager, attributesGenerator])
    {
        QueryBuilder = queryBuilder;
        _queryManager = queryManager;
        _attributesGenerator = attributesGenerator;
        ProvideOut(GetStreamsOfQuery);
        ProvideOut(GetAttributes, "Attributes");
    }

    /// <summary>
    /// Get list of all streams which the query has.
    /// </summary>
    /// <returns></returns>
    private IImmutableList<IEntity> GetStreamsOfQuery()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();

        var dataFactory = DataFactory.SpawnNew(options: new()
        {
            TitleField = StreamsType.Name.ToString(),
            TypeName = QueryStreamsContentType
        });

        var result = Query?.Out
                         .OrderBy(stream => stream.Key)
                         .Select(stream
                             => dataFactory.Create(new Dictionary<string, object>
                             {
                                 { StreamsType.Name.ToString(), stream.Key }
                             }))
                         .ToImmutableList()
                     ?? [];
        return l.Return(result, $"{result.Count}");
    }
        
    /// <summary>
    /// List the attributes of
    /// </summary>
    /// <returns></returns>
    private IImmutableList<IEntity> GetAttributes()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();

        // no query can happen if the name was blank
        var query = Query;
        if (query == null)
            return l.Return([], "null");

        // check that _query has the stream name
        if (StreamName.IsEmptyOrWs())
            return l.Return([], "Stream Name empty");

        var streamNames = StreamName.CsvToArrayWithoutEmpty();
        var realStreams = streamNames.Where(query.Out.ContainsKey).ToList();

        if (!realStreams.Any())
            return l.Return([], "can't find stream name in query");

        var results = realStreams
            .SelectMany(sName =>
            {
                // Prepare Inspection DataSource
                var attribInfo = _attributesGenerator.New(attach: query);

                // For non-default streams, it should know about the desired stream
                // So we're attaching the other stream to the inspection
                if (sName != StreamDefaultName)
                    attribInfo.Attach(StreamDefaultName, query, sName);

                return attribInfo.List;
            })
            .DistinctBy(e => e.GetBestTitle())
            .ToImmutableList();

        return l.Return(results, $"{results.Count}");
    }

    private IDataSource Query => _q.Get(BuildQuery);
    private readonly GetOnce<IDataSource> _q = new();


    private IDataSource BuildQuery()
    {
        var l = Log.Fn<IDataSource>();

        Configuration.Parse();

        var qName = QueryName;
        if (string.IsNullOrWhiteSpace(qName))
            return l.ReturnNull("empty name");

        // important, use "Name" and not get-best-title, as some queries may not be correctly typed, so missing title-info
        var found = _queryManager.Value.FindQuery(this, qName, recurseParents: 3)
                    ?? throw new($"Can't build query info - query not found '{qName}'");

        var builtQuery = QueryBuilder.GetDataSourceForTesting(QueryBuilder.Create(found, AppId),
            lookUps: Configuration.LookUpEngine);
        return l.Return(builtQuery.Main);
    }

}