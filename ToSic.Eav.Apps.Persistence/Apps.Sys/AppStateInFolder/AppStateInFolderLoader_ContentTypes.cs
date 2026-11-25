using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Persistence.File;

namespace ToSic.Eav.Apps.Sys.AppStateInFolder;

partial class AppStateInFolderLoader
{
    private (ICollection<FileSystemLoader> Loaders, ICollection<IContentType> ContentTypes, ICollection<IEntity> Entities)
        LoadGlobalContentTypes(ICollection<FileSystemLoader> loaders, IAppStateCache appState)
    {
        var l = Log.Fn<(ICollection<FileSystemLoader>, ICollection<IContentType>, ICollection<IEntity>)>(timer: true);

        // 1. Create a delayed content type provider, which will
        //    later on give the generated sub-entities their content type (which may not exist during deserialization)
        var delayedContentTypeProvider = new DeferredContentTypeProvider(Log.IfDetails(LogSettings));

        // 2. Prepare settings to ensure CT-definitions and entities inside the content-types and their attributes
        //    can look up their correct content type, metadata and relationships
        //    This is necessary because the serializer will not yet know what App it's for,
        //    since the App state is not fully loaded
        var deSerializeSettings = new JsonDeSerializationSettings
        {
            ContentTypeProvider = delayedContentTypeProvider,
            MetadataSource = appState,
            RelationshipsSource = appState,
        };

        // 3. Load all content types from all loaders, incl. any additional entities they may have
        var newSets = loaders
            .Select(ldr =>
            {
                var prevOpts = ldr.GetLatestOptions();
                var prevJsonOpts = prevOpts.JsonReaderOptions;
                var newOpts = prevOpts with { JsonReaderOptions = prevJsonOpts with { DeserializationSettings = deSerializeSettings } };
                
                // Create new special loader just for this, with the new configuration.
                var tempLoader = _fslGenerator.New(newOpts);
                var ctWithEntities = tempLoader.ContentTypesWithEntities();

                // Create a new loader with the old settings, but using the counter-updates from the previous changes
                var updatedLoader = _fslGenerator.New(tempLoader.GetLatestOptions() with
                {
                    JsonReaderOptions = prevJsonOpts,
                });
                return (updatedLoader, ctWithEntities);
            })
            .ToListOpt();

        var newLoaders = newSets
            .Select(set => set.updatedLoader)
            .ToListOpt();

        var newCtsPairs = newSets
            .Select(set => set.ctWithEntities)
            .ToListOpt();

        // 5. Finalize all created entities to ensure they reference the newly created content-types
        var newTypes = newCtsPairs
            .SelectMany(set => set.ContentTypes)
            .ToListOpt();
        var newEntities = newCtsPairs
            .SelectMany(set => set.Entities)
            .ToListOpt();
        delayedContentTypeProvider.AddTypes(newTypes);
        var types = delayedContentTypeProvider.SetTypesOfContentTypeParts();
        delayedContentTypeProvider.SetTypesOfOtherEntities(newEntities);

        return l.Return((newLoaders, types, newEntities), $"found {types.Count} types; {newEntities.Count} entities");
    }

}