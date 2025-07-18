﻿using System.Collections;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.DataSource.Sys.Caching;

namespace ToSic.Eav.DataSource.Sys.Streams;

/// <summary>
/// A DataStream to get Entities when needed
/// </summary>
/// <param name="cache"></param>
/// <param name="source">The DataSource providing Entities when needed</param>
/// <param name="name">Name of this Stream</param>
/// <param name="listDelegate">Function which gets Entities</param>
/// <param name="enableAutoCaching"></param>
[PrivateApi]
public class DataStream(
    LazySvc<IDataSourceCacheService> cache,
    IDataSource source,
    string name,
    Func<IImmutableList<IEntity>>? listDelegate = null,
    bool enableAutoCaching = false,
    string? scope = default
    ) : IDataStream
{

    #region Alternate Constructor using IEnumerable<IEntity> instead of IImmutableList<IEntity>

    /// <summary>
    /// Constructs a new DataStream
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="source">The DataSource providing Entities when needed</param>
    /// <param name="name">Name of this Stream</param>
    /// <param name="listDelegate">Function which gets Entities</param>
    /// <param name="enableAutoCaching"></param>
    public DataStream(LazySvc<IDataSourceCacheService> cache, IDataSource source, string name, Func<IEnumerable<IEntity>>? listDelegate = null, bool enableAutoCaching = false)
        : this(cache, source, name, ConvertDelegate(listDelegate), enableAutoCaching) { }

    private static Func<IImmutableList<IEntity>>? ConvertDelegate(Func<IEnumerable<IEntity>>? original)
    {
        if (original == null)
            return null;
        return () =>
        {
            var initialResult = original();
            return initialResult as IImmutableList<IEntity> ?? initialResult.ToImmutableOpt();
        };
    }

    #endregion


    #region Self-Caching and Results-Persistence Properties / Features

    /// <inheritdoc />
    /// <summary>
    /// Place the stream in the cache if wanted, by default not
    /// </summary>
    public bool AutoCaching { get; set; } = enableAutoCaching;

    /// <inheritdoc />
    /// <summary>
    /// Default cache duration is 1 day = 3600 * 24
    /// </summary>
    public int CacheDurationInSeconds { get; set; } = 3600 * 24; // one day, since by default if it caches, it would check upstream for cache-reload


    /// <inheritdoc />
    /// <summary>
    /// Kill the cache if the source data is newer than the cache-stamped data
    /// </summary>
    public bool CacheRefreshOnSourceRefresh { get; set; } = true;

    /// <summary>
    /// Special cache duration for streams returning an error.
    /// Default to `0`. Possible values:
    /// 
    /// * `0` cache for 3 times as-long-as-it-takes to generate source data (delay retry on slow source)
    /// * `-1` don't cache at all and retry immediately
    /// * any other number: cache longer, e.g. if the source needs long to generate.
    /// </summary>
    public int CacheErrorDurationInSeconds { get; set; } = 0;

    /// <inheritdoc />
    public string Scope { get; protected internal set; } = scope ?? ScopeConstants.Default;

    /// <summary>
    /// Provide access to the CacheKey - so it could be overridden if necessary without using the stream underneath it
    /// </summary>
    public virtual DataStreamCacheStatus Caching => field ??= new(Source, Source, Name);

    #endregion



    #region Get Dictionary and Get List

    /// <inheritdoc />
    public IEnumerable<IEntity> List => _list.Get(GetList)!;

    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>($"{nameof(Name)}:{Name}", timer: true);
        // Check if it's in the cache - and if yes, if it's still valid and should be re-used --> return if found
        if (!AutoCaching)
            return l.Return(ReadUnderlyingList(), $"read; no {nameof(AutoCaching)}");

        var cacheItem = cache.Value.ListCache.GetOrBuild(this, ReadUnderlyingList, CacheDurationInSeconds, CacheErrorDurationInSeconds);
        return l.Return(cacheItem.List, $"with {nameof(AutoCaching)}");

    }

    /// <inheritdoc />
    public void ResetStream() => _list.Reset();

    /// <summary>
    /// A temporary result list - must be a List, because otherwise
    /// there's a high risk of IEnumerable signatures with functions being stored inside.
    /// </summary>
    /// <remarks>
    /// Where possible, it will be an ImmutableSmartList wrapping an ImmutableArray for maximum performance.
    /// </remarks>
    private readonly GetOnce<IImmutableList<IEntity>> _list = new();

    /// <summary>
    /// Assemble the list - from the initially configured ListDelegate
    /// </summary>
    /// <returns></returns>
    private IImmutableList<IEntity> ReadUnderlyingList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        // try to use the built-in Entities-Delegate, but if not defined, use other delegate; just make sure we test both, to prevent infinite loops

        IImmutableList<IEntity> CreateErr(string title, string message, Exception? ex = default)
            => Source.Error.Create(source: Source, title: title, message: message, exception: ex).ToImmutableOpt();

        if (listDelegate == null)
            return l.ReturnAsError(CreateErr("Error loading Stream",
                "Can't load stream - no delegate found to supply it"));

        try
        {
            var resultList = ImmutableSmartList.Wrap(listDelegate());
            return l.ReturnAsOk(resultList);
        }
        catch (InvalidOperationException invEx) // this is a special exception - for example when using SQL. Pass it on to enable proper testing
        {
            return l.ReturnAsError(CreateErr("InvalidOperationException", "See details", invEx));
        }
        catch (Exception ex)
        {
            return l.ReturnAsError(CreateErr("Error getting Stream / reading underlying list",
                $"Error getting List of Stream.\nStream Name: {Name}\nDataSource Name: {Source.Name}", ex));
        }
    }
    #endregion

    /// <inheritdoc />
    /// <summary>
    /// The source which holds this stream
    /// </summary>
    public IDataSource Source { get; } = source;

    /// <inheritdoc />
    /// <summary>
    /// Name - usually the name within the Out-dictionary of the source. For identification and for use in caching-IDs and similar
    /// </summary>
    public string Name { get; } = name;


    #region Support for IEnumerable<IEntity>

    //IEnumerator<ICanBeEntity> IEnumerable<ICanBeEntity>.GetEnumerator() => List.GetEnumerator();

    public IEnumerator<IEntity> GetEnumerator() => List.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

    #endregion Support for IEnumerable<IEntity>

    public ILog Log => Source?.Log!; // Note that it can be null, but most commands on Log are null-safe


    public IDataSourceLink Link => _link.Get(() => new DataSourceLink(null, dataSource: Source, stream: this, outName: Name))!;
    private readonly GetOnce<IDataSourceLink> _link = new();
}