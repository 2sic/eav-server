using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Lib.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps;

[PrivateApi("don't publish this - too internal, special, complicated")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppRelationshipManager: SynchronizedList<EntityRelationship>
{
    internal AppRelationshipManager(AppState upstream) : base(upstream, () => Rebuild(upstream))
    {
    }

    private static ImmutableList<EntityRelationship> Rebuild(AppState appState)
    {
        // todo: could be optimized (minor)
        // atm guid-relationships (like in json-objects) 
        // will have multiple lookups - first to find the json, then to add to relationship index

        var cache = new List<EntityRelationship>();
        var index = appState.Index;
        foreach (var entity in appState.List)
        {
            var lazyEntityValues = entity.Attributes
                .Select(a => a.Value)
                .Where(a => a is IAttribute<IEnumerable<IEntity>>)
                .Cast<IAttribute<IEnumerable<IEntity>>>()
                .Select(a => a.Typed.First().TypedContents as LazyEntitiesSource)
                .Where(tc => tc != null);
            foreach (var value in lazyEntityValues)
            foreach (var val in value.EntityIds.Where(e => e != null))
                Add(index, cache, entity.EntityId, val);
        }

        return cache.ToImmutableList();
    }

    private static void Add(IReadOnlyDictionary<int, IEntity> lookup, List<EntityRelationship> list, int parent, int? child)
    {
        //var lookup = appState.Index;
        if (lookup.ContainsKey(parent) &&
            (!child.HasValue || lookup.ContainsKey(child.Value)))
            list.Add(new EntityRelationship(lookup[parent], child.HasValue ? lookup[child.Value] : null));
    }
}