using ToSic.Eav.Apps.State;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Internal.Loaders;

namespace ToSic.Eav.Persistence.File;

partial class AppLoader
{
    public (List<IContentType> ContentTypes, List<IEntity> Entities) LoadGlobalContentTypes(IAppStateCache appState)
    {
        var l = Log.Fn<(List<IContentType> ContentTypes, List<IEntity> Entities)>();

        // Set TypeID seed for loader so each loaded type has a unique ID
        var loaderIndex = 1;
        Loaders.ForEach(ldr => ldr.TypeIdSeed = FsDataConstants.GlobalContentTypeMin + FsDataConstants.GlobalContentTypeSourceSkip * loaderIndex++);

        // 3 - return content types
        var delayedContentTypeProvider = new DeferredContentTypeProvider(Log);

        // #SharedFieldDefinition
        var deSerializeSettings = new JsonDeSerializationSettings
        {
            EntityContentTypeProvider = delayedContentTypeProvider,
            CtAttributeMetadataAppState = appState,
            SharedEntitiesSource = appState,
        };

        var newTypes = Loaders.Select(ldr =>
            {
                ldr.Serializer.DeserializationSettings = deSerializeSettings;
                var ctWithEntities = ldr.ContentTypesWithEntities();
                var result = ctWithEntities.ContentTypes; // ldr.ContentTypes();
                ldr.Serializer.DeserializationSettings = null;
                return ctWithEntities;
            })
            .ToList();

        delayedContentTypeProvider.AddTypes(newTypes.SelectMany(set => set.ContentTypes).ToList());

        var entities = newTypes.SelectMany(set => set.Entities).ToList();

        var types = delayedContentTypeProvider.ProcessSubEntitiesOnTypes(entities);

        return l.Return((types, entities), $"found {types.Count} types; {entities.Count} entities");
    }

}