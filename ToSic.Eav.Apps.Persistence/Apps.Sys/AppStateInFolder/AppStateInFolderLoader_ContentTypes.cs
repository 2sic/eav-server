using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Sys.Performance;

namespace ToSic.Eav.Apps.Sys.AppStateInFolder;

partial class AppStateInFolderLoader
{
    private (List<IContentType> ContentTypes, List<IEntity> Entities) LoadGlobalContentTypes(IAppStateCache appState)
    {
        var l = Log.Fn<(List<IContentType> ContentTypes, List<IEntity> Entities)>(timer: true);

        // 1. Set TypeID seed for loader so each loaded type has a unique ID
        var loaderIndex = 1;
        Loaders.ForEach(ldr => ldr.TypeIdSeed = GlobalAppIdConstants.GlobalContentTypeMin + GlobalAppIdConstants.GlobalContentTypeSourceSkip * loaderIndex++);

        // 2. Create a delayed content type provider, which will
        //    later on give the generated sub-entities their content type (which may not exist during deserialization)
        var delayedContentTypeProvider = new DeferredContentTypeProvider(Log.IfDetails(LogSettings));

        // 3. Prepare settings to ensure CT-definitions and entities inside the content-types and their attributes
        //    can look up their correct content type, metadata and relationships
        //    This is necessary because the serializer will not yet know what App it's for,
        //    since the App state is not fully loaded
        var deSerializeSettings = new JsonDeSerializationSettings
        {
            ContentTypeProvider = delayedContentTypeProvider,
            MetadataSource = appState,
            RelationshipsSource = appState,
        };

        // 4. Load all content types from all loaders, incl. any additional entities they may have
        var newSets = Loaders
            .Select(ldr =>
            {
                ldr.Serializer.DeserializationSettings = deSerializeSettings;
                var ctWithEntities = ldr.ContentTypesWithEntities();
                ldr.Serializer.DeserializationSettings = null;
                return ctWithEntities;
            })
            .ToListOpt();

        // 5. Finalize all created entities to ensure they reference the newly created content-types
        var newTypes = newSets
            .SelectMany(set => set.ContentTypes)
            .ToListOpt();
        var newEntities = newSets
            .SelectMany(set => set.Entities)
            .ToList();
        delayedContentTypeProvider.AddTypes(newTypes);
        var types = delayedContentTypeProvider.SetTypesOfContentTypeParts();
        delayedContentTypeProvider.SetTypesOfOtherEntities(newEntities);

        return l.Return((types, newEntities), $"found {types.Count} types; {newEntities.Count} entities");
    }

}