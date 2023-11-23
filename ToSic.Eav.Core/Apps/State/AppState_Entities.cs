using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {
        /// <summary>
        /// The simple list of <em>all</em> entities, used everywhere
        /// </summary>
        public IImmutableList<IEntity> List => (ListCache ??= BuildList()).List;
        internal SynchronizedEntityList ListCache;

        private SynchronizedEntityList BuildList()
        {
            // todo: check if feature is enabled #SharedAppFeatureEnabled
            var buildFn = ParentApp.InheritEntities
                ? () => Index.Values.Concat(ParentApp.Entities).ToImmutableList()
                : (Func<IImmutableList<IEntity>>)(() => Index.Values.ToImmutableList());

            var syncList = new SynchronizedEntityList(this, buildFn);

            return syncList;
        }


        IEnumerable<IEntity> IEntitiesSource.List => List;

        internal Dictionary<int, IEntity> Index { get; } = new();

        /// <summary>
        /// Add an entity to the cache. Should only be used by EAV code
        /// </summary>
        [PrivateApi("Only internal use")]
        public void Add(IEntity newEntity, int? publishedId, bool log)
        {
            if (!Loading)
                throw new Exception("trying to add entity, but not in loading state. set that first!");

            if (newEntity.RepositoryId == 0)
                throw new Exception("Entities without real ID not supported yet");

            RemoveObsoleteDraft(newEntity, log);
            MapDraftToPublished(newEntity as Entity, publishedId, log); // this is not immutable, but probably not an issue because it is not in the index yet
            Index[newEntity.RepositoryId] = newEntity; // add like this, it could also be an update
            _metadataManager.Register(newEntity, true);

            if (FirstLoadCompleted)
                DynamicUpdatesCount++;

            if (log) Log.A($"added entity {newEntity.EntityId} for published {publishedId}; dyn-update#{DynamicUpdatesCount}");
        }

        /// <summary>
        /// Removes an entity from the cache. Should only be used by EAV code
        /// </summary>
        /// <remarks>
        /// Introduced in v15.05 to reduce work on entity delete.
        /// In past we PurgeApp in whole on each entity delete.
        /// This should be much faster, but side effects are possible.
        /// </remarks>
        [PrivateApi("Only internal use")]
        public void Remove(int[] repositoryIds, bool log)
        {
            if (repositoryIds == null || repositoryIds.Length == 0) return;
            Load(() =>
            {
                foreach (var id in repositoryIds)
                {
                    // Remove any drafts that are related if necessary
                    if (Index.TryGetValue(id, out var oldEntity))
                    {
                        // RemoveObsoleteDraft(oldEntity, log);

                        // Removes the entity from list
                        _metadataManager.Register(oldEntity, false);

                        //// Removes reference to draft entity from published
                        //if (GetPublished(oldEntity) is Entity publishEntity) 
                        //    publishEntity.DraftEntity = null;
                    }

                    Index.Remove(id);

                    if (log) Log.A($"removed entity {id}");
                }
            });
        }

        /// <summary>
        /// Reset all item storages and indexes
        /// </summary>
        internal void RemoveAllItems()
        {
            if (!Loading)
                throw new Exception("trying to init metadata, but not in loading state. set that first!");
            Log.A("remove all items");
            Index.Clear();
            _metadataManager.Reset();
        }

    }
}
