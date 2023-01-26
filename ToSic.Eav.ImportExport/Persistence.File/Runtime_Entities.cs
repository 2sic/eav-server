using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime
    {
        protected int EntityIdSeed = FsDataConstants.GlobalEntityIdMin;

        private List<IEntity> LoadAndDeduplicateEntities(AppState appState) =>
            Log.Func(l =>
            {
                // This will be the source of all relationships
                // In the end it must contain all entities - but not deleted ones...
                var listOfEntitiesForRelationshipMapping = new List<IEntity>();

                var entitySets = FsDataConstants.EntityItemFolders
                    .Select(folder => new EntitySetsToLoad
                    {
                        Folder = folder,
                        Entities = LoadGlobalEntitiesFromAllLoaders(folder, listOfEntitiesForRelationshipMapping,
                            appState) ?? new List<IEntity>()
                    })
                    .ToList();

                l.A($"Found {entitySets.Count} sets");

                // Deduplicate entities 
                var entities = entitySets.SelectMany(es => es.Entities).ToList();
                var entitiesDeduplicated = entities
                    .GroupBy(x => x.EntityGuid)
                    .Select(g => g.Last())
                    // After Deduplicating we want to order them, in case we need to debug something
                    .OrderBy(e => e.EntityId)
                    .ToList();
                var duplicates = entities.Count - entitiesDeduplicated.Count;
                l.A($"Found {duplicates} duplicate entities from {entities.Count} resulting with {entitiesDeduplicated.Count}");

                // Reset list of entities which will be used to find related entities
                listOfEntitiesForRelationshipMapping.Clear();
                listOfEntitiesForRelationshipMapping.AddRange(entitiesDeduplicated);

                return (entitiesDeduplicated, "ok");
            });

        private List<IEntity> LoadGlobalEntitiesFromAllLoaders(string groupIdentifier, List<IEntity> listForRelationships, AppState appState) =>
            Log.Func(l =>
            {
                if (!FsDataConstants.EntityItemFolders.Any(f => f.Equals(groupIdentifier)))
                    throw new ArgumentOutOfRangeException(nameof(groupIdentifier),
                        "atm we can only load items of type " + string.Join("/", FsDataConstants.EntityItemFolders));

                // Get items
                var entities = new List<IEntity>();
                foreach (var loader in Loaders)
                {
                    loader.ResetSerializer(appState);
                    entities.AddRange(loader.Entities(groupIdentifier, EntityIdSeed, listForRelationships));
                    EntityIdSeed += FsDataConstants.GlobalEntitySourceSkip; // update the seed for next rounds or other uses of the seed
                }

                l.A($"{entities.Count} items of type {groupIdentifier}");
                return entities;
            });

        
        internal class EntitySetsToLoad
        {
            public string Folder;
            public List<IEntity> Entities;
        }
    }
}
