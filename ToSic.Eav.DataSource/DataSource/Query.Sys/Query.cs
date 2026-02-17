using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Eav.DataSource.Sys.Streams;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sys.Engines;
using static System.String;
using static System.StringComparer;


namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Provides a Query as a data-source.
/// </summary>
/// <remarks>
/// Note that this source provides access to the query, but it won't run unless you access the data.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
[method: PrivateApi]
public class Query(DataSourceBase.Dependencies services, LazySvc<QueryFactory> queryBuilder, LazySvc<QueryDefinitionFactory> queryDefBuilder)
    : DataSourceBase(services, $"{DataSourceConstantsInternal.LogPrefix}.Query", connect: [queryBuilder, queryDefBuilder]), IQuery,
        ICacheAlsoAffectsOut
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

    /// <summary>
    /// Initialize a full query object. This is necessary for it to work
    /// </summary>
    /// <returns></returns>
    [PrivateApi]
    public void Init(int zoneId, int appId, IEntity queryDef, ILookUpEngine? lookUpEngineOrNull, IDataSource? source = null)
    {
        var l = Log.Fn($"{zoneId}/{appId}");

        ZoneId = zoneId;
        AppId = appId;
        Definition = queryDefBuilder.Value.Create(appId, queryDef);
        
        if (lookUpEngineOrNull != null)
            ((DataSourceConfiguration)Configuration).LookUpEngine = lookUpEngineOrNull;

        // hook up in, just in case we get parameters from an In
        if (source == null)
        {
            l.A("found target for Query, will attach");
            _inSource = source;
        }

        l.Done();
    }

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
        var l = Log.Fn<(IDataSource Source, StreamDictionary Out)>(message: $"Query: '{Definition.Title}'", timer: true);
        // Step 1: Resolve the params from outside, where x=[Params:y] should come from the outer Params
        // and the current In
        var resolvedParams = Configuration.LookUpEngine.LookUp(Definition.ParamsDic);

        // now provide an override source for this
        var paramsOverride = new LookUpInDictionary(DataSourceConstants.ParamsSourceName, resolvedParams);
        var queryInfos = queryBuilder.Value.Create(Definition, Configuration.LookUpEngine, [paramsOverride]);
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
        => Params(QueryDefinitionParams.GenerateParamsDic(list, Log));

    /// <inheritdoc />
    public void Params(IDictionary<string, string> values)
    {
        foreach (var qP in values)
            Params(qP.Key, qP.Value);
    }

    /// <inheritdoc />
    public IDictionary<string, string> Params() => Definition.ParamsDic;

    // # RemoveDataSourceReset v21
    /// <inheritdoc />
    [PrivateApi("should be removed soon")]
    public void Reset()
    {
        var l = Log.Fn("Reset query and update RequiresRebuildOfOut");
        Definition.Reset();
        _requiresRebuildOfOut = true;
        l.Done();
    }

}