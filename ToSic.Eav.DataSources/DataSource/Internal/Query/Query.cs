using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSource.Streams;
using ToSic.Eav.DataSource.Streams.Internal;
using ToSic.Eav.LookUp;
using ToSic.Lib.Helpers;
using static System.String;
using static System.StringComparer;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSource.Internal.Query;

/// <summary>
/// Provides a data-source to a query, but won't assemble/compile the query unless accessed (lazy). 
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class Query : DataSourceBase, IQuery, ICacheAlsoAffectsOut
{
    #region Configuration-properties

    /// <inheritdoc />
    public QueryDefinition Definition { get; private set; }

    private StreamDictionary OutWritable
    {
        get => _outWritable ??= new(Services.CacheService);
        set => _outWritable = value;
    }

    private StreamDictionary _outWritable;
    private bool _requiresRebuildOfOut = true;

    /// <summary>
    /// Standard out. Note that the Out is not prepared until accessed the first time,
    /// when it will auto-assembles the query
    /// </summary>
    public override IReadOnlyDictionary<string, IDataStream> Out
    {
        get
        {
            if (!_requiresRebuildOfOut) return OutWritable.AsReadOnly();
            CreateOutWithAllStreams();
            _requiresRebuildOfOut = false;
            return OutWritable.AsReadOnly();
        }
    }
    #endregion

    #region Internal Source - mainly for debugging or advanced uses of a query

    /// <summary>
    /// Inner source - mainly for debugging etc.
    /// </summary>
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
    public Query Init(int zoneId, int appId, IEntity queryDef, ILookUpEngine configSource, IDataSource source = null)
    {
        ZoneId = zoneId;
        AppId = appId;
        Definition = QueryBuilder.Create(queryDef, appId);
        this.Init(configSource);

        // hook up in, just in case we get parameters from an In
        if (source == null) return this;

        Log.A("found target for Query, will attach");
        _inSource = source;
        return this;
    }

    [PublicApi]
    public override IReadOnlyDictionary<string, IDataStream> In => _inSource?.In ?? _in;
    private readonly IReadOnlyDictionary<string, IDataStream> _in = new Dictionary<string, IDataStream>(InvariantCultureIgnoreCase);
    private IDataSource _inSource;


    /// <summary>
    /// Create a stream for each data-type
    /// </summary>
    private void CreateOutWithAllStreams() => Log.Do(timer: true, message: $"Query: '{Definition.Entity.GetBestTitle()}'", action: () =>
    {
        // Step 1: Resolve the params from outside, where x=[Params:y] should come from the outer Params
        // and the current In
        var resolvedParams = Configuration.LookUpEngine.LookUp(Definition.Params);

        // now provide an override source for this
        var paramsOverride = new LookUpInDictionary(DataSourceConstants.ParamsSourceName, resolvedParams);
        var queryInfos = QueryBuilder.BuildQuery(Definition, Configuration.LookUpEngine,
            [paramsOverride]);
        _source = queryInfos.Main;
        OutWritable = new(this, _source.Out);
    });

    private QueryBuilder QueryBuilder => _queryBuilder.Get(() => _queryBuilderLazy.Value);
    private readonly GetOnce<QueryBuilder> _queryBuilder = new();



    /// <inheritdoc />
    public void Params(string key, string value) => Log.Do($"{key}, {value}", l =>
    {
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
        var l = Log.Fn("Reset query and update RequiresRebuildOfOut");
        Definition.Reset();
        _requiresRebuildOfOut = true;
        l.Done();
    }

    ///// <summary>
    ///// Override PurgeList, because we don't really have In streams, unless we use parameters. 
    ///// </summary>
    ///// <param name="cascade"></param>
    //public override void PurgeList(bool cascade = false)
    //{
    //    var l = Log.Fn($"{cascade} - on {GetType().Name}");
    //    // PurgeList on all In, as would usually happen
    //    // This will only purge query-in used for parameter
    //    //base.PurgeList(cascade);
    //    Services.DsCacheSvc.Value.UnCache(0, this, cascade);
    //    //Services.DsCacheSvc.Value.UnCache(this, cascade, Out);

    //    //l.A("Now purge the lists which the Query has on the Out");
    //    //foreach (var stream in Source.Out)
    //    //    stream.Value.PurgeList(cascade);
    //    //if (!Source.Out.Any()) l.A("No streams on Source.Out found to clear");

    //    Reset();
    //    //l.A("Update RequiresRebuildOfOut");
    //    //_requiresRebuildOfOut = true;
    //    l.Done();
    //}
}