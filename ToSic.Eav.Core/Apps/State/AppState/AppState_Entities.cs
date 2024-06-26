﻿using System.Collections.Immutable;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;

namespace ToSic.Eav.Apps.State;

partial class AppState: IEntitiesSource
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

    ///// <summary>
    ///// Add an entity to the cache. Should only be used by EAV code
    ///// </summary>
    //[PrivateApi("Only internal use")]
    //internal void Add(IEntity newEntity, int? publishedId, bool log)
    //{
    //    if (!Loading)
    //        throw new Exception("trying to add entity, but not in loading state. set that first!");

    //    if (newEntity.RepositoryId == 0)
    //        throw new Exception("Entities without real ID not supported yet");

    //    RemoveObsoleteDraft(newEntity, log);
    //    MapDraftToPublished(newEntity as Entity, publishedId, log); // this is not immutable, but probably not an issue because it is not in the index yet
    //    Index[newEntity.RepositoryId] = newEntity; // add like this, it could also be an update
    //    _metadataManager.Register(newEntity, true);

    //    if (FirstLoadCompleted)
    //        DynamicUpdatesCount++;

    //    if (log) Log.A($"added entity {newEntity.EntityId} for published {publishedId}; dyn-update#{DynamicUpdatesCount}");
    //}
    

    ///// <summary>
    ///// Reset all item storages and indexes
    ///// </summary>
    //private void RemoveAllItems()
    //{
    //    if (!Loading)
    //        throw new Exception("trying to init metadata, but not in loading state. set that first!");
    //    Log.A("remove all items");
    //    Index.Clear();
    //    _metadataManager.Reset();
    //}

}