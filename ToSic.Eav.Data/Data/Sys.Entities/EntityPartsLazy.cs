using ToSic.Eav.Apps.Sys;
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

    /// <summary>
    /// Will generate a Parts-Builder for entities which belong to an App.
    /// Like entities being loaded into the App-State
    /// or entities which are JSON loaded and will be placed in an App state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public static EntityPartsLazy ForAppAndOptionalMetadata(IAppStateCache? source = default, IEnumerable<IEntity>? metadata = default)
        => new(
            entity => new EntityRelationships(entity, source),
            getMetadataOf: metadata != default
                ? CreateMetadataOfItems(metadata)
                : CreateMetadataOfAppSources(source)
        );

    private static IMetadata EmptyGetMetadataOf(Guid guid, string title)
        => new Metadata<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title, source: new MetadataSourceEmpty());

    private static Func<Guid, string, IMetadata> CreateMetadataOfAppSources(IHasMetadataSourceAndExpiring? appSource)
        => (guid, title) => new Metadata<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title, source: new MetadataSourceApp(appSource));

    private static Func<Guid, string, IMetadata> CreateMetadataOfItems(IEnumerable<IEntity> items)
        => (guid, title) => new Metadata<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title, source: new MetadataSourceItems(items));

    public static Func<TKey, string, IMetadata> ReUseMetadataFunc<TKey>(IMetadata original) 
        => (_, _) => original;

    public static Func<TKey, string, IMetadata> CloneMetadataFunc<TKey>(IMetadata original)
    {
        var asInternal = (IMetadataInternals)original;
        return (key, title) => new Metadata<TKey>(targetType: asInternal.TargetType, key: key, title: title, source: asInternal.Source);
    }

    public static Func<TKey, string, IMetadata> CloneMetadataFunc<TKey>(IMetadata original, List<IEntity>? items)
    {
        if (items == null) 
            return CloneMetadataFunc<TKey>(original);
        var asInternal = (IMetadataInternals)original;
        return (key, title) => new Metadata<TKey>(
            targetType: asInternal.TargetType,
            key: key,
            title: title,
            source: new MetadataSourceItems(items));
    }

    //public static Func<TKey, string, IMetadata> CloneMetadataFunc<TKey>(
    //    IMetadata original,
    //    List<IEntity>? items = default,
    //    IHasMetadataSourceAndExpiring? appSource = default,
    //    Func<IHasMetadataSourceAndExpiring>? deferredSource = default
    //)
    //{
    //    var asInternal = (IMetadataInternals)original;
    //    var specs = asInternal.GetCloneSpecs();
    //    return (key, title) => new Metadata<TKey>(
    //        targetType: asInternal.TargetType,
    //        key: key,
    //        title: title,
    //        // source: asInternal.Source,
    //        items: items ?? specs.list,
    //        appSource: appSource ?? specs.appSource,
    //        deferredSource: deferredSource ?? specs.deferredSource);
    //}

}