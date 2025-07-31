using System.Collections.Immutable;
using System.Collections.ObjectModel;
using ToSic.Eav.Data.Entities.Sys;

namespace ToSic.Eav.Apps.Sys.State;
internal class AppStateEntities(AppState appState)
{

    /// <summary>
    /// The simple list of <em>all</em> entities, used everywhere.
    /// Also includes inherited entities from the parent app if configured to do so.
    /// </summary>
    public IImmutableList<IEntity> ImmutableList => ListCache.List;

    [field: AllowNull, MaybeNull]
    internal SynchronizedEntityList ListCache => field ??= BuildList();

    [field: AllowNull, MaybeNull]
    internal ReadOnlyDictionary<int, IEntity> Index
    {
        get => field ??= RebuildIndex();
        set;
    }

    public int Count => IndexRaw.Count;

    private Dictionary<int, IEntity> IndexRaw { get; } = [];

    private ReadOnlyDictionary<int, IEntity> RebuildIndex()
        => new(IndexRaw);

    public void AddOrReplace(int index, IEntity entity)
    {
        IndexRaw[entity.EntityId] = entity;
        Index = null!; // Update the index after adding a new entity
    }

    public void Remove(int id)
    {
        IndexRaw.Remove(id);
        Index = null!;
    }

    public void Clear()
    {
        IndexRaw.Clear();
        Index = null!;
    }

    public bool Any() => IndexRaw.Any();

    internal SynchronizedEntityList BuildList()
    {
        // todo: check if feature is enabled #SharedAppFeatureEnabled
        var buildFn = ((ParentAppState)appState.ParentApp).InheritEntities
            ? () => Index.Values.Concat(appState.ParentApp.Entities).ToImmutableOpt()
            : (Func<IImmutableList<IEntity>>)(() => Index.Values.ToImmutableOpt());

        var syncList = new SynchronizedEntityList(appState, buildFn);

        return syncList;
    }

}
