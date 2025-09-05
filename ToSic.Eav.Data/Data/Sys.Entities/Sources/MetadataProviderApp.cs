using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Data.Sys.Entities.Sources;

public class MetadataProviderApp(IHasMetadataSourceAndExpiring sourceApp): IMetadataProvider
{
    public DirectEntitiesSource? List => null;
    public IHasMetadataSourceAndExpiring LookupSource => sourceApp;
    public long CacheTimestamp => sourceApp.CacheTimestamp;
    public bool CacheChanged() => CacheChanged(CacheTimestamp);
    public bool CacheChanged(long dependentTimeStamp) => sourceApp.CacheChanged(dependentTimeStamp);
}