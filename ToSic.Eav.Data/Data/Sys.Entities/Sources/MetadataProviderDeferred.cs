using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Data.Sys.Entities.Sources;

internal class MetadataProviderDeferred(Func<IHasMetadataSourceAndExpiring> sourceDeferred) : IMetadataProvider
{
    public DirectEntitiesSource? List => null;

    [field: AllowNull, MaybeNull]
    public IHasMetadataSourceAndExpiring LookupSource => field ??= sourceDeferred.Invoke();

    public long CacheTimestamp => LookupSource.CacheTimestamp;
    public bool CacheChanged() => CacheChanged(CacheTimestamp);

    public bool CacheChanged(long dependentTimeStamp) => LookupSource.CacheChanged(dependentTimeStamp);

}