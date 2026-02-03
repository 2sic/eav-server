using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;
namespace ToSic.Eav.Data.Build;

/// <summary>
/// WIP v21
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityConnectionBuilder
{

    public EntityPartsLazy UseMetadata(IEnumerable<IEntity> metadata)
        => UseOptional(null, metadata);

    public EntityPartsLazy UseApp(IAppStateCache? source)
        => UseOptional(source, null);

    /// <summary>
    /// Will generate a Parts-Builder for entities which belong to an App.
    /// Like entities being loaded into the App-State
    /// or entities which are JSON loaded and will be placed in an App state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public EntityPartsLazy UseOptional(IAppStateCache? source = default, IEnumerable<IEntity>? metadata = default)
        => new(
            entity => new EntityRelationships(entity, source),
            getMetadataOf: metadata != default
                ? CreateMetadataOfItems(metadata)
                : CreateMetadataOfAppSources(source)
        );

    private static Func<Guid, string, IMetadata> CreateMetadataOfAppSources(IHasMetadataSourceAndExpiring? appSource)
        => (guid, title) => new Metadata<Guid>(
            targetType: (int)TargetTypes.Entity,
            key: guid,
            title: title,
            source: MetadataProvider.Create(source: appSource)
        );

    private static Func<Guid, string, IMetadata> CreateMetadataOfItems(IEnumerable<IEntity> items)
        => (guid, title) => new Metadata<Guid>(
            targetType: (int)TargetTypes.Entity,
            key: guid,
            title: title,
            source: MetadataProvider.Create(items)
        );

}