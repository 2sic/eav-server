using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Data.Sys.Entities;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityPartsLazy
{
    internal readonly Func<IEntity, IEntityRelationships> GetRelationshipDelegate;
    internal readonly Func<Guid, string, IMetadata> GetMetadataOfDelegate;

    public EntityPartsLazy(
        Func<IEntity, IEntityRelationships>? getRelationshipManager = default,
        Func<Guid, string, IMetadata>? getMetadataOf = default)
    {
        GetRelationshipDelegate = getRelationshipManager ?? (e => new EntityRelationships(e, null, null));
        GetMetadataOfDelegate = getMetadataOf ?? EmptyGetMetadataOf;
    }

    private static IMetadata EmptyGetMetadataOf(Guid guid, string title)
        => new Metadata<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title, source: new MetadataProviderEmpty());

    internal static Func<TKey, string, IMetadata> ReUseMetadataFunc<TKey>(IMetadata original) 
        => (_, _) => original;

    internal static Func<TKey, string, IMetadata> CloneMetadataFunc<TKey>(IMetadata original)
    {
        var asInternal = (IMetadataInternals)original;
        return (key, title) => new Metadata<TKey>(targetType: asInternal.TargetType, key: key, title: title, source: asInternal.Source);
    }

    internal static Func<TKey, string, IMetadata> CloneMetadataFunc<TKey>(IMetadata original, List<IEntity>? items)
    {
        if (items == null) 
            return CloneMetadataFunc<TKey>(original);
        var asInternal = (IMetadataInternals)original;
        return (key, title) => new Metadata<TKey>(
            targetType: asInternal.TargetType,
            key: key,
            title: title,
            source: MetadataProvider.Create(items));
    }
}