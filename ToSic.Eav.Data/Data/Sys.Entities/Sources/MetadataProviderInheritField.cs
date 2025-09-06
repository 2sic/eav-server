using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Data.Sys.Entities.Sources;

/// <summary>
/// Experimental - try to retrieve the metadata from another attribute - for json loaded content-type attributes with inheritMetadata.
/// </summary>
/// <param name="sourceApp"></param>
public class MetadataProviderInheritField(IEntitiesSource sourceApp, Guid attributeGuid): IMetadataProvider
{
    public IHasMetadataSourceAndExpiring? LookupSource => null;

    public DirectEntitiesSource List => GetItems();

    private ImmutableEntitiesSource? _list;
    private ImmutableEntitiesSource? _itemsWhenNotReady;

    ///// <summary>
    ///// Will track if the data for the field metadata is final - if true, won't try to reload.
    ///// This is because on early access during build, it will not be available yet, so it will return an empty list.
    ///// </summary>
    //private bool dataIsFinal;

    private ImmutableEntitiesSource GetItems()
    {
        // If the list exists, then we already filled it in the final state
        if (_list != null && !CacheChanged())
            return _list;

        // If the app is not ready, return empty list - but remember that this is not final
        if (sourceApp is not IAppStateCache { FirstLoadCompleted: true } appStateCache)
            return _itemsWhenNotReady ??= new([]);

        var sourceAttribute = appStateCache.ContentTypes
            .SelectMany(ct => ct.Attributes)
            .FirstOrDefault(a => a.Guid == attributeGuid);

        if (sourceAttribute?.Metadata is not ContentTypeAttributeMetadata sourceMd)
            return _list = new([]);

        return _list = new([.. sourceMd.AllWithHidden]);
    }

    public long CacheTimestamp => sourceApp?.CacheTimestamp ?? 0;
    public bool CacheChanged() => CacheChanged(CacheTimestamp);
    public bool CacheChanged(long dependentTimeStamp) => sourceApp?.CacheChanged(dependentTimeStamp) ?? false;
}