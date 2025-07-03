using ToSic.Eav.DataSource.Streams.Internal;
using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sys.Engines;
using static System.String;
using static System.StringComparer;


namespace ToSic.Eav.DataSource.Sys.Query;

/// <summary>
/// Provides a data-source to a query, but won't assemble/compile the query unless accessed (lazy). 
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class Query : DataSourceBase, IQuery, ICacheAlsoAffectsOut
{
    #region Configuration-properties

    /// <inheritdoc />
    public QueryDefinition Definition { get; private set; } = null!;

    private bool _requiresRebuildOfOut = true;

    /// <summary>
    /// Standard out. Note that the Out is not prepared until accessed the first time,
    /// when it will auto-assembles the query
    /// </summary>
    public override IReadOnlyDictionary<string, IDataStream> Out
        => Data.Out.AsReadOnly();

    #endregion

    #region Internal Source - mainly for debugging or advanced uses of a query

    /// <summary>
    /// Inner source - mainly for debugging etc.
    /// </summary>
    [PrivateApi]
    public IDataSource Source
        => Data.Source;

    #endregion

    /// <inheritdoc />
    [PrivateApi]
    public Query(Dependencies services, LazySvc<QueryBuilder> queryBuilder) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Query", connect: [queryBuilder])
    {
        _queryBuilderLazy = queryBuilder;
    }
    private readonly LazySvc<QueryBuilder> _queryBuilderLazy;

    /// <summary>
    /// Initialize a full query object. This is necessary for it to work
    /// </summary>
    /// <returns></returns>
    [PrivateApi]
    public Query Init(int zoneId, int appId, IEntity queryDef, ILookUpEngine? lookUpEngineOrNull, IDataSource? source = null)
    {
        ZoneId = zoneId;
        AppId = appId;
        Definition = _queryBuilderLazy.Value.Create(queryDef, appId);
        this.Init(lookUpEngineOrNull);

        // hook up in, just in case we get parameters from an In
        if (source == null)
            return this;

        Log.A("found target for Query, will attach");
        _inSource = source;
        return this;
    }

    [PublicApi]
    [field: AllowNull, MaybeNull]
    public override IReadOnlyDictionary<string, IDataStream> In
        => _inSource?.In ?? (field ??= new Dictionary<string, IDataStream>(InvariantCultureIgnoreCase));

    private IDataSource? _inSource;



    private (IDataSource Source, StreamDictionary Out) Data => _data ??= CreateOutWithAllStreams();
    private (IDataSource Source, StreamDictionary Out)? _data;

    /// <summary>
    /// Create a stream for each data-type
    /// </summary>
    private (IDataSource Source, StreamDictionary Out) CreateOutWithAllStreams()
    {
        var l = Log.Fn<(IDataSource Source, StreamDictionary Out)>(message: $"Query: '{Definition.Entity.GetBestTitle()}'", timer: true);
        // Step 1: Resolve the params from outside, where x=[Params:y] should come from the outer Params
        // and the current In
        var resolvedParams = Configuration.LookUpEngine.LookUp(Definition.ParamsDic);

        // now provide an override source for this
        var paramsOverride = new LookUpInDictionary(DataSourceConstants.ParamsSourceName, resolvedParams);
        var queryInfos = _queryBuilderLazy.Value.BuildQuery(Definition, Configuration.LookUpEngine,
            [paramsOverride]);
        var source = queryInfos.Main;
        var outWritable = new StreamDictionary(this, Services.CacheService, streams: source.Out);
        return l.Return((source, outWritable));
    }


    /// <inheritdoc />
    public void Params(string key, string? value)
    {
        var l = Log.Fn($"{key}, {value}");
        if (IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));
        // if the query has already been built, and we're changing a value, make sure we'll regenerate the results
        if (!_requiresRebuildOfOut)
        {
            l.A("Can't set param - query already compiled, error");
            throw new("Can't set param any more, the query has already been compiled. " +
                      "Always set params before accessing the data. " +
                      "To Re-Run the query with other params, call Reset() first.");
        }

        Definition.ParamsDic[key] = value ?? "";
        l.Done();
    }

    /// <inheritdoc />
    public void Params(string key, object? value)
        => Params(key, value?.ToString());


    /// <inheritdoc />
    public void Params(string list)
        => Params(QueryDefinition.GenerateParamsDic(list, Log));

    /// <inheritdoc />
    public void Params(IDictionary<string, string> values)
    {
        foreach (var qP in values)
            Params(qP.Key, qP.Value);
    }

    /// <inheritdoc />
    public IDictionary<string, string> Params() => Definition.ParamsDic;

    /// <inheritdoc />
    public void Reset()
    {
        var l = Log.Fn("Reset query and update RequiresRebuildOfOut");
        Definition.Reset();
        _requiresRebuildOfOut = true;
        l.Done();
    }

}