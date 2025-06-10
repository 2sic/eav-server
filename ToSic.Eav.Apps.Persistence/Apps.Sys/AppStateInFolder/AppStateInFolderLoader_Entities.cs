using ToSic.Eav.Persistence.File;

namespace ToSic.Eav.Apps.Sys.AppStateInFolder;

partial class AppStateInFolderLoader
{
    private ICollection<IEntity> LoadGlobalEntities(IAppReader appReader)
    {
        var l = Log.Fn<ICollection<IEntity>>($"appId:{appReader.AppId}", timer: true);
        // Set TypeID seed for loader so each loaded type has a unique ID
        var loaderIndex = 1;
        foreach (var ldr in Loaders)
            ldr.EntityIdSeed = GlobalAppIdConstants.GlobalEntityIdMin + GlobalAppIdConstants.GlobalEntitySourceSkip * loaderIndex++;

        // This will be the source of all relationships
        // In the end it must contain all entities - but not deleted ones...
        var final = DirectEntitiesSource.Using(relationships =>
        {
            var entitySets = AppDataFoldersConstants.EntityItemFolders
                .Select(folder => new EntitySetsToLoad
                {
                    Folder = folder,
                    Entities = LoadGlobalEntitiesFromAllLoaders(folder, relationships.Source, appReader) ?? []
                })
                .ToListOpt();

            l.A($"Found {entitySets.Count} sets");

            var entitiesUnique = DeduplicateAndLogStats(entitySets);

            // Reset list of entities which will be used to find related entities
            relationships.List.Clear();
            relationships.List.AddRange(entitiesUnique);
            return entitiesUnique;
        });

        return l.ReturnAsOk(final);
    }

    private ICollection<IEntity> DeduplicateAndLogStats(ICollection<EntitySetsToLoad> entitySets)
    {
        var l = Log.Fn<ICollection<IEntity>>();
        // Deduplicate entities 
        var entities = entitySets
            .SelectMany(es => es.Entities)
            .ToListOpt();
        var entitiesGroupedByGuid = entities
            .GroupBy(x => x.EntityGuid)
            .ToListOpt();
        var entitiesUnique = entitiesGroupedByGuid
            .Select(g => g.Last())
            // After Deduplicating we want to order them, in case we need to debug something
            .OrderBy(e => e.EntityId)
            .ToListOpt();

        // Log duplicates if logging is active (Log != null)
        if (l != null)
        {
            var duplicates = entities.Count - entitiesUnique.Count;

            l.A($"Found {duplicates} duplicate entities from {entities.Count} resulting with {entitiesUnique.Count}");
            foreach (var dupl in entitiesGroupedByGuid.Where(g => g.Count() > 1))
                l.A($"Removed a duplicate of: {dupl.Key}");

            // Detailed debug - log all IDs because we seem to have duplicate IDs (bug)
            l.A("Showing unique IDs");
            foreach (var e in entitiesUnique)
                l.A($"Id: {e.EntityId} ({e.EntityGuid})");
        }

        return l.Return(entitiesUnique, $"final: {entitiesUnique.Count}");
    }

    private ICollection<IEntity> LoadGlobalEntitiesFromAllLoaders(string groupIdentifier, DirectEntitiesSource relationshipSource, IAppReader appReader) 
    {
        var l = Log.Fn<ICollection<IEntity>>($"groupIdentifier:{groupIdentifier}", timer: true);
        if (!AppDataFoldersConstants.EntityItemFolders.Any(f => f.Equals(groupIdentifier)))
            throw new ArgumentOutOfRangeException(nameof(groupIdentifier),
                "atm we can only load items of type " + string.Join("/", AppDataFoldersConstants.EntityItemFolders));

        // Get items
        var entities = new List<IEntity>();
        foreach (var loader in Loaders)
        {
            loader.ResetSerializer(appReader);
            var newEntities = loader.Entities(groupIdentifier, relationshipSource);
            entities.AddRange(newEntities);
        }

        return l.Return(entities, $"{entities.Count} items of type {groupIdentifier}");
    }

        
    internal class EntitySetsToLoad
    {
        public string Folder;
        public ICollection<IEntity> Entities;
    }
}