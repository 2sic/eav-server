using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {
        /// <summary>
        /// The simple list of <em>all</em> entities, used everywhere
        /// </summary>
        public IImmutableList<IEntity> List
            => (ListCache ?? (ListCache = new SynchronizedEntityList(this, () => Index.Values.ToImmutableArray()))).List;
        internal SynchronizedEntityList ListCache;

        IEnumerable<IEntity> IEntitiesSource.List => List;

        internal Dictionary<int, IEntity> Index { get; } = new Dictionary<int, IEntity>();

        /// <summary>
        /// Add an entity to the cache. Should only be used by EAV code
        /// </summary>
        [PrivateApi("Only internal use")]
        public void Add(Entity newEntity, int? publishedId, bool log)
        {
            if (!Loading)
                throw new Exception("trying to add entity, but not in loading state. set that first!");

            if (newEntity.RepositoryId == 0)
                throw new Exception("Entities without real ID not supported yet");

            RemoveObsoleteDraft(newEntity, log);
            Index[newEntity.RepositoryId] = newEntity; // add like this, it could also be an update
            MapDraftToPublished(newEntity, publishedId, log);
            _metadataManager.Register(newEntity);

            if (FirstLoadCompleted)
                DynamicUpdatesCount++;

            if (log) Log.Add($"added entity {newEntity.EntityId} for published {publishedId}; dyn-update#{DynamicUpdatesCount}");
        }

        /// <summary>
        /// Reset all item storages and indexes
        /// </summary>
        internal void RemoveAllItems()
        {
            if (!Loading)
                throw new Exception("trying to init metadata, but not in loading state. set that first!");
            Log.Add("remove all items");
            Index.Clear();
            _metadataManager.Reset();
        }

    }
}
