using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Data.Sys.Entities.Sources;
public static class MetadataProvider
{
    public static IMetadataProvider Create(IEnumerable<IEntity>? items = null, IHasMetadataSourceAndExpiring? source = null, Func<IHasMetadataSourceAndExpiring>? sourceDeferred = null)
    {
        if (items != null)
            return new MetadataProviderDirect(items);

        if (source != null)
            return new MetadataProviderApp(source);

        if (sourceDeferred != null)
            return new MetadataProviderDeferred(sourceDeferred);

        return new MetadataProviderEmpty();
    }
}

//[PrivateApi("keep secret for now, only used in Metadata and it's not sure if we should re-use this")]
//[ShowApiWhenReleased(ShowApiMode.Never)]
//public class MetadataSourceWipOld(
//    DirectEntitiesSource? sourceDirect = null,
//    IHasMetadataSourceAndExpiring? sourceApp = null,
//    Func<IHasMetadataSourceAndExpiring>? sourceDeferred = null)
//    : IMetadataProvider
//{
//    public DirectEntitiesSource? SourceDirect { get; } = sourceDirect;
//    private IHasMetadataSourceAndExpiring? SourceApp { get; } = sourceApp;
//    private Func<IHasMetadataSourceAndExpiring>? SourceDeferred { get; } = sourceDeferred;

//    // #CleanUpMetadataVarieties 2025-09-05 2dm
//    //public bool UseSource { get; set; } = true;

//    public IHasMetadataSourceAndExpiring? MainSource => _mainSource.Get(() => SourceApp ?? SourceDeferred?.Invoke());
//    private readonly GetOnce<IHasMetadataSourceAndExpiring?> _mainSource = new();

//    private ICacheExpiring? ExpirySource => _expirySourceReal.Get(() => (ICacheExpiring?)SourceDirect ?? MainSource);
//    private readonly GetOnce<ICacheExpiring?> _expirySourceReal = new();

//    /// <summary>
//    /// The cache has a very "old" timestamp, so it's never newer than a dependent
//    /// </summary>
//    public long CacheTimestamp
//        => ExpirySource?.CacheTimestamp ?? 0;

//    public bool CacheChanged(long dependentTimeStamp)
//        => ExpirySource?.CacheChanged(dependentTimeStamp) == true;

//    public bool CacheChanged() => CacheChanged(CacheTimestamp);
//}
