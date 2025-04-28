using System.Collections.ObjectModel;
using ToSic.Eav.DataSource.Streams.Internal;
using ToSic.Lib.Helpers;
using static System.StringComparer;

namespace ToSic.Eav.DataSources.Caching;

/// <summary>
/// Special DataSource which automatically caches everything it's given.
/// It's Used to optimize queries, so that heavier calculations don't need to be repeated if another request with the same signature is used. <br/>
/// Internally it asks all up-stream DataSources what factors would determine their caching.
/// So if part of the supplying DataSources would have a changed parameter (like a different filter), it will still run the full query and cache the results again. 
/// </summary>
/// <remarks>
/// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
/// * note that the above change is actually a breaking change, but since this is such an advanced DataSource, we assume it's not used in dynamic code.
/// </remarks>
[VisualQuery(
    NiceName = "Cache Streams",
    UiHint = "Cache all streams based on some rules",
    Icon = DataSourceIcons.HistoryOff,
    Type = DataSourceType.Cache, 
    NameId = "ToSic.Eav.DataSources.Caching.CacheAllStreams, ToSic.Eav.DataSources",
    DynamicOut = true,
    DynamicIn = true,
    ConfigurationType = "|Config ToSic.Eav.DataSources.Caches.CacheAllStreams",
    NameIds =
    [
        "ToSic.Eav.DataSources.Caches.CacheAllStreams, ToSic.Eav.DataSources"
    ],
    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-CacheAllStreams")]
[PublicApi]
public class CacheAllStreams : DataSourceBase
{
    #region Configuration-properties

    /// <summary>
    /// How long to keep these streams in the cache.
    /// Default is `0` - meaning fall back to 1 day
    /// </summary>
    [Configuration(Fallback = 0, CacheRelevant = false)]
    public int CacheDurationInSeconds => Configuration.GetThis(0);

    /// <summary>
    /// If a source-refresh should trigger a cache rebuild
    /// </summary>
    [Configuration(Fallback = true, CacheRelevant = false)]
    public bool RefreshOnSourceRefresh => Configuration.GetThis(true);

    /// <summary>
    /// Perform a cache rebuild async. 
    /// </summary>
    [Configuration(Fallback = false, CacheRelevant = false)]
    public bool ReturnCacheWhileRefreshing => Configuration.GetThis(false);

    #endregion

    #region Dynamic Out

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, IDataStream> Out => _out.Get(() =>
    {
        Configuration.Parse();

        // attach all missing streams, now that Out is used the first time
        // note that some streams were already added because of the DeferredOut
        var outList = new Dictionary<string, IDataStream>(InvariantCultureIgnoreCase);
        foreach (var dataStream in In.Where(s => !outList.ContainsKey(s.Key)))
            outList.Add(dataStream.Key, StreamWithCaching(dataStream.Key));

        return new ReadOnlyDictionary<string, IDataStream>(outList);
    });
    private readonly GetOnce<IReadOnlyDictionary<string, IDataStream>> _out = new();

    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new App DataSource
    /// </summary>
    [PrivateApi]
    public CacheAllStreams(MyServices services): base(services, $"{DataSourceConstantsInternal.LogPrefix}.CachAl")
    {
        // this one is unusual, so don't pre-attach a default data stream
    }

    private IDataStream StreamWithCaching(string name)
    {
        var outStream = new DataStream(Services.CacheService, this, name,  () => In[name].List, true);

        // only set if a value other than 0 (= default) was given
        if (CacheDurationInSeconds != 0)
            outStream.CacheDurationInSeconds = CacheDurationInSeconds;
        outStream.CacheRefreshOnSourceRefresh = RefreshOnSourceRefresh;
        return outStream;
    }
        
}