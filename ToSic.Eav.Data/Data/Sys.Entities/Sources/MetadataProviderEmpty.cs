using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Data.Sys.Entities.Sources;

internal class MetadataProviderEmpty : IMetadataProvider
{
    [field: AllowNull, MaybeNull]
    public DirectEntitiesSource List => field ??= new ImmutableEntitiesSource([]);
    public IHasMetadataSourceAndExpiring? LookupSource => null;

    public long CacheTimestamp => 0;
    public bool CacheChanged() => false;

    public bool CacheChanged(long dependentTimeStamp) => false;
}