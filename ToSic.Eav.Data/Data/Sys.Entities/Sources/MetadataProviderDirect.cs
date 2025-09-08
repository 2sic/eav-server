using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Data.Sys.Entities.Sources;

internal class MetadataProviderDirect(DirectEntitiesSource sourceDirect) : IMetadataProvider
{
    public MetadataProviderDirect(IEnumerable<IEntity> items) : this(new ImmutableEntitiesSource(items.ToImmutableOpt()))
    { }

    public DirectEntitiesSource List => sourceDirect;
    public IHasMetadataSourceAndExpiring? LookupSource => null;

    public long CacheTimestamp => 0;
    public bool CacheChanged() => false;
    public bool CacheChanged(long dependentTimeStamp) => false;
}