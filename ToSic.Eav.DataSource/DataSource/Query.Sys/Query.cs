using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Eav.DataSource.Sys.Streams;
using ToSic.Eav.LookUp.Sys.Engines;
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
    : DataSourceBase(services, $"{DataSourceConstantsInternal.LogPrefix}.Query", connect: [queryBuilder, queryDefBuilder]),
        IQuery,
        ICacheAlsoAffectsOut,
        IServiceWithSetup<Query.Options>
{
    #region Options and Setup

    [PrivateApi]
    public record Options(IAppIdentity Identity, IEntity QueryDef, ILookUpEngine? LookUpEngine);

    /// <summary>
    /// Initialize a full query object. This is necessary for it to work
    /// </summary>
    /// <returns></returns>
    [PrivateApi]
    public void Setup(Options options)
    {
        var (identity, queryDef, lookUpEngineOrNull) = options;
        var l = Log.Fn($"{identity.Show()}");

        ZoneId = identity.ZoneId;
        AppId = identity.AppId;
        Definition = queryDefBuilder.Value.Create(identity.AppId, queryDef);

        if (lookUpEngineOrNull != null)
            ((DataSourceConfiguration)Configuration).LookUpEngine = lookUpEngineOrNull;

        l.Done();
    }

    #endregion



    #region Configuration-properties

    /// <inheritdoc />
    public QueryDefinition Definition { get; private set; } = null!;

    private bool _requiresRebuildOfOut = true;

    #endregion



    #region Out and In

    /// <summary>
    /// Standard out. Note that the Out is not prepared until accessed the first time,
    /// when it will auto-assembles the query
    /// </summary>
    public override IReadOnlyDictionary<string, IDataStream> Out
        => field ??= CreateOutWithAllStreams().Out.AsReadOnly();

    /// <summary>
    /// Create a stream for each data-type
    /// </summary>
    private (IDataSource Source, StreamDictionary Out) CreateOutWithAllStreams()
    {
        var l = Log.Fn<(IDataSource Source, StreamDictionary Out)>(message: $"Query: '{Definition.Title}'", timer: true);

        var queryInfos = queryBuilder.Value.CreateWithParams(Definition, Configuration.LookUpEngine);
        var outWritable = new StreamDictionary(this, Services.CacheService, streams: queryInfos.Main.Out);
        return l.Return((queryInfos.Main, outWritable));
    }

    
    [field: AllowNull, MaybeNull]
    public override IReadOnlyDictionary<string, IDataStream> In
        => /*_inSource?.In ?? #DropInOnQuery */ (field ??= new Dictionary<string, IDataStream>(InvariantCultureIgnoreCase));
    // #DropInOnQuery
    //private IDataSource? _inSource;

    #endregion




    #region Params() API

    private QueryParamsHelper ParamsHelper => field ??= new(Definition, () => !_requiresRebuildOfOut, Log);

    /// <inheritdoc />
    public void Params(string key, string? value) => ParamsHelper.Params(key, value);

    /// <inheritdoc />
    public void Params(string key, object? value) => ParamsHelper.Params(key, value?.ToString());


    /// <inheritdoc />
    public void Params(string list) => ParamsHelper.Params(list);

    /// <inheritdoc />
    public void Params(IDictionary<string, string> values) => ParamsHelper.Params(values);

    /// <inheritdoc />
    public IDictionary<string, string> Params() => Definition.ParamsDic;

    #endregion

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