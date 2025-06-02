using ToSic.Eav.Apps;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Internal.Loaders;

namespace ToSic.Eav.Persistence.File;

partial class AppStateInFolderLoader
{
    private List<IEntity> LoadGlobalEntities(IAppReader appReader)
    {
        var l = Log.Fn<List<IEntity>>($"appId:{appReader.AppId}", timer: true);
        // Set TypeID seed for loader so each loaded type has a unique ID
        var loaderIndex = 1;
        Loaders.ForEach(ldr => ldr.EntityIdSeed = FsDataConstants.GlobalEntityIdMin + FsDataConstants.GlobalEntitySourceSkip * loaderIndex++);

        // This will be the source of all relationships
        // In the end it must contain all entities - but not deleted ones...
        var final = DirectEntitiesSource.Using(relationships =>
        {
            var entitySets = FsDataConstants.EntityItemFolders
                .Select(folder => new EntitySetsToLoad
                {
                    Folder = folder,
                    Entities = LoadGlobalEntitiesFromAllLoaders(folder, relationships.Source, appReader) ?? []
                })
                .ToList();

            l.A($"Found {entitySets.Count} sets");

            var entitiesUnique = DeduplicateAndLogStats(entitySets);

            // Reset list of entities which will be used to find related entities
            relationships.List.Clear();
            relationships.List.AddRange(entitiesUnique);
            return entitiesUnique;
        });

        return l.ReturnAsOk(final);
    }

    private List<IEntity> DeduplicateAndLogStats(List<EntitySetsToLoad> entitySets)
    {
        var l = Log.Fn<List<IEntity>>();
        // Deduplicate entities 
        var entities = entitySets
            .SelectMany(es => es.Entities)
            .ToList();
        var entitiesGroupedByGuid = entities
            .GroupBy(x => x.EntityGuid)
            .ToList();
        var entitiesUnique = entitiesGroupedByGuid
            .Select(g => g.Last())
            // After Deduplicating we want to order them, in case we need to debug something
            .OrderBy(e => e.EntityId)
            .ToList();

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

    private List<IEntity> LoadGlobalEntitiesFromAllLoaders(string groupIdentifier, DirectEntitiesSource relationshipSource, IAppReader appReader) 
    {
        var l = Log.Fn<List<IEntity>>($"groupIdentifier:{groupIdentifier}", timer: true);
        if (!FsDataConstants.EntityItemFolders.Any(f => f.Equals(groupIdentifier)))
            throw new ArgumentOutOfRangeException(nameof(groupIdentifier),
                "atm we can only load items of type " + string.Join("/", FsDataConstants.EntityItemFolders));

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
        public List<IEntity> Entities;
    }
}