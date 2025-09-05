using ToSic.Eav.Metadata.Sys;
using ToSic.Sys.Caching;

namespace ToSic.Eav.Data.Sys.Entities.Sources;

[PrivateApi("keep secret for now, only used in Metadata and it's not sure if we should re-use this")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MetadataSourceWipOld(
    DirectEntitiesSource? sourceDirect = null,
    IHasMetadataSourceAndExpiring? sourceApp = null,
    Func<IHasMetadataSourceAndExpiring>? sourceDeferred = null)
    : IMetadataSource<IHasMetadataSourceAndExpiring>
{
    public DirectEntitiesSource? SourceDirect { get; } = sourceDirect;
    private IHasMetadataSourceAndExpiring? SourceApp { get; } = sourceApp;
    private Func<IHasMetadataSourceAndExpiring>? SourceDeferred { get; } = sourceDeferred;

    // #CleanUpMetadataVarieties 2025-09-05 2dm
    //public bool UseSource { get; set; } = true;

    public IHasMetadataSourceAndExpiring? MainSource => _mainSource.Get(() => SourceApp ?? SourceDeferred?.Invoke());
    private readonly GetOnce<IHasMetadataSourceAndExpiring?> _mainSource = new();

    private ICacheExpiring? ExpirySource => _expirySourceReal.Get(() => (ICacheExpiring?)SourceDirect ?? MainSource);
    private readonly GetOnce<ICacheExpiring?> _expirySourceReal = new();

    /// <summary>
    /// The cache has a very "old" timestamp, so it's never newer than a dependent
    /// </summary>
    public long CacheTimestamp
        => ExpirySource?.CacheTimestamp ?? 0;

    public bool CacheChanged(long dependentTimeStamp)
        => ExpirySource?.CacheChanged(dependentTimeStamp) == true;

    public bool CacheChanged() => CacheChanged(CacheTimestamp);
}

public class MetadataSourceApp(IHasMetadataSourceAndExpiring sourceApp): MetadataSourceWipOld(sourceApp: sourceApp);
public class MetadataSourceDeferred(Func<IHasMetadataSourceAndExpiring> sourceDeferred) : MetadataSourceWipOld(sourceDeferred: sourceDeferred);

public class MetadataSourceItems(DirectEntitiesSource sourceDirect) : MetadataSourceWipOld(sourceDirect: sourceDirect)
{
    public MetadataSourceItems(IEnumerable<IEntity> items) : this(new ImmutableEntitiesSource(items.ToImmutableOpt()))
    { }
}

public class MetadataSourceEmpty() : MetadataSourceWipOld(new ImmutableEntitiesSource([]));

public interface IMetadataSource<out TSource> : ICacheExpiring, ICacheDependent
{
    public DirectEntitiesSource? SourceDirect { get; }

    public TSource? MainSource { get; }

    //public TSource? SourceApp { get; }

    //public Func<TSource>? SourceDeferred { get; }
}