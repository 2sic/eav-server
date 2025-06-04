using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Entities.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityPartsLazy
{
    internal readonly Func<IEntity, IEntityRelationships> GetRelationshipDelegate;
    internal readonly Func<Guid, string, IMetadataOf> GetMetadataOfDelegate;

    public EntityPartsLazy(
        Func<IEntity, IEntityRelationships> getRelationshipManager = default,
        Func<Guid, string, IMetadataOf> getMetadataOf = default)
    {
        GetRelationshipDelegate = getRelationshipManager ?? (e => new EntityRelationships(e, null, null));
        GetMetadataOfDelegate = getMetadataOf ?? EmptyGetMetadataOf;
    }

    /// <summary>
    /// Will generate a Parts-Builder for entities which belong to an App.
    /// Like entities being loaded into the App-State
    /// or entities which are JSON loaded and will be placed in an App state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public static EntityPartsLazy ForAppAndOptionalMetadata(IAppStateCache source = default, List<IEntity> metadata = default)
        => new(
            entity => new EntityRelationships(entity, source),
            getMetadataOf: metadata != default
                ? CreateMetadataOfItems(metadata)
                : CreateMetadataOfAppSources(source)
        );

    private static IMetadataOf EmptyGetMetadataOf(Guid guid, string title)
        => new MetadataOf<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title);

    private static Func<Guid, string, IMetadataOf> CreateMetadataOfAppSources(IHasMetadataSourceAndExpiring appSource)
        => (guid, title) => new MetadataOf<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title,
            appSource: appSource);

    private static Func<Guid, string, IMetadataOf> CreateMetadataOfItems(List<IEntity> items)
        => (guid, title) => new MetadataOf<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title, items: items);

    public static Func<TKey, string, IMetadataOf> ReUseMetadataFunc<TKey>(IMetadataOf original) 
        => (key, title) => original;

    public static Func<TKey, string, IMetadataOf> CloneMetadataFunc<TKey>(IMetadataOf original,
        List<IEntity> items = default,
        IHasMetadataSourceAndExpiring appSource = default,
        Func<IHasMetadataSourceAndExpiring> deferredSource = default
    )
    {
        var specs = ((IMetadataInternals)original).GetCloneSpecs();
        return (key, title) => new MetadataOf<TKey>(targetType: specs.TargetType, key: key, title: title,
            items: items ?? specs.list,
            appSource: appSource ?? specs.appSource,
            deferredSource: deferredSource ?? specs.deferredSource);
    }

}