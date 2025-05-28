using System.Collections.Immutable;
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

    internal Dictionary<int, IEntity> Index { get; } = [];
    
}