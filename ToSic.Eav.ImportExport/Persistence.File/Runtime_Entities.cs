using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime
    {
        protected int EntityIdSeed = FsDataConstants.GlobalEntityIdMin;

        private List<IEntity> LoadGlobalItems(string groupIdentifier, List<IEntity> listForRelationships)
        {
            var wrapLog = Log.Fn<List<IEntity>>(groupIdentifier);

            if(!FsDataConstants.EntityItemFolders.Any(f => f.Equals(groupIdentifier)))
                throw new ArgumentOutOfRangeException(nameof(groupIdentifier), "atm we can only load items of type " + string.Join("/", FsDataConstants.EntityItemFolders));

            // Get items
            var entities = new List<IEntity>();
            foreach (var l in Loaders)
            {
                entities.AddRange(l.Entities(groupIdentifier, EntityIdSeed, listForRelationships));
                EntityIdSeed += FsDataConstants.GlobalEntitySourceSkip; // update the seed for next rounds or other uses of the seed
            }

            return wrapLog.Return(entities, $"{entities.Count} items of type {groupIdentifier}");
        }


        private List<EntitySetsToLoad> LoadAndDeduplicateEntitySets() => Log.Func<List<EntitySetsToLoad>>(l =>
        {
            // This will be the source of all relationships
            // In the end it must contain all entities - but not deleted ones...
            var listOfEntitiesForRelationshipMapping = new List<IEntity>();

            //var l = Log.Fn<List<EntitySetsToLoad>>();
            var entitySets = FsDataConstants.EntityItemFolders
                .Select(folder => new EntitySetsToLoad
                {
                    Folder = folder,
                    Entities = LoadGlobalItems(folder, listOfEntitiesForRelationshipMapping) ?? new List<IEntity>()
                })
                .ToList();

            l.A($"Found {entitySets.Count} sets");

            // FIND duplicates in own set...
            entitySets.ForEach(eSet =>
            {
                eSet.Entities = eSet.Entities.GroupBy(x => x.EntityGuid).Select(g => g.Last()).ToList();
            });

            // Reset list of entities which will be used to find related entities
            listOfEntitiesForRelationshipMapping.Clear();
            listOfEntitiesForRelationshipMapping.AddRange(entitySets.SelectMany(es => es.Entities));

            return (entitySets, "ok");
        });

        //var l = Log.Fn<List<EntitySetsToLoad>>();
            //var entitySets = FsDataConstants.EntityItemFolders
            //    .Select(folder => new EntitySetsToLoad
            //    {
            //        Folder = folder,
            //        Entities = LoadGlobalItems(folder) ?? new List<IEntity>()
            //    })
            //    .ToList();

            //Log.A($"Found {entitySets.Count} sets");

            //// Deduplicate - remove items in first sets which are overriden by subsequent sets...
            //for (var i = 0; i < entitySets.Count - 1; i++) // Important: skip the last one
            //{
            //    var currentSet = entitySets[i];
            //    var allDuplicates = new List<IEntity>();
            //    foreach (var laterSet in entitySets.Skip(i + 1))
            //    {
            //        var matches = currentSet.Entities.Where(e =>
            //            laterSet.Entities.Any(el => el.EntityGuid == e.EntityGuid));
            //        allDuplicates.AddRange(matches);
            //    }

            //    Log.A($"Found {allDuplicates.Count} duplicates - will remove");
            //    if (allDuplicates.Any())
            //        allDuplicates.ForEach(d => currentSet.Entities.Remove(d));

            //}

            //// FIND duplicates in own set...
            //entitySets.ForEach(currentSet => {
            //    currentSet.Entities = currentSet.Entities.GroupBy(x => x.EntityGuid).Select(g => g.Last()).ToList();
            //});

            //return l.Return(entitySets);
        internal class EntitySetsToLoad
        {
            public string Folder;
            public List<IEntity> Entities;
        }
    }
}
