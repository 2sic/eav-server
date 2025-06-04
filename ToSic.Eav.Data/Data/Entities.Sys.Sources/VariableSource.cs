using ToSic.Lib.Helpers;
using ToSic.Sys.Caching;

namespace ToSic.Eav.Data.Source;

[PrivateApi("keep secret for now, only used in Metadata and it's not sure if we should re-use this")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class VariableSource<TSource>(
    DirectEntitiesSource sourceDirect = default,
    TSource sourceApp = default,
    Func<TSource> sourceDeferred = default)
    : ICacheExpiring, ICacheDependent
    where TSource : class, ICacheExpiring
{
    public DirectEntitiesSource SourceDirect { get; } = sourceDirect;
    public TSource SourceApp { get; } = sourceApp;
    public Func<TSource> SourceDeferred { get; } = sourceDeferred;

    public bool UseSource { get; set; } = true;

    public TSource MainSource => _mainSource.Get(() => SourceApp ?? SourceDeferred?.Invoke());
    private readonly GetOnce<TSource> _mainSource = new();

    public ICacheExpiring ExpirySource => _expirySourceReal.Get(() => (ICacheExpiring)SourceDirect ?? MainSource);
    private readonly GetOnce<ICacheExpiring> _expirySourceReal = new();

    /// <summary>
    /// The cache has a very "old" timestamp, so it's never newer than a dependent
    /// </summary>
    public long CacheTimestamp => ExpirySource?.CacheTimestamp ?? 0;

    public bool CacheChanged(long dependentTimeStamp) => ExpirySource?.CacheChanged(dependentTimeStamp) == true;

    public bool CacheChanged() => CacheChanged(CacheTimestamp);
}