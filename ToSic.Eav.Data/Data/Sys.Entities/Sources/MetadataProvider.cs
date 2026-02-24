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

    public static IMetadataProvider Create(IEnumerable<IEntity>? items) =>
        items != null! /* paranoid */
            ? new MetadataProviderDirect(items)
            : new MetadataProviderEmpty();

    public static IMetadataProvider Create(IHasMetadataSourceAndExpiring? source) =>
        source != null! /* paranoid */
            ? new MetadataProviderApp(source)
            : new MetadataProviderEmpty();

    public static IMetadataProvider Create(Func<IHasMetadataSourceAndExpiring> sourceDeferred) =>
        sourceDeferred != null! /* paranoid */
            ? new MetadataProviderDeferred(sourceDeferred)
            : new MetadataProviderEmpty();
}
