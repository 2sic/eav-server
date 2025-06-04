using System.Collections.Immutable;
using ToSic.Eav.Data.Attributes.Sys;
using ToSic.Eav.Data.Entities.Sys.Sources;
using ToSic.Sys.Caching.Synchronized;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.State;

[PrivateApi("don't publish this - too internal, special, complicated")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class AppRelationshipManager(AppState upstream): SynchronizedList<IEntityRelationship>(upstream, () => Rebuild(upstream))
{
    private static ImmutableList<IEntityRelationship> Rebuild(AppState appState)
    {
        // todo: could be optimized (minor)
        // atm guid-relationships (like in json-objects) 
        // will have multiple lookups - first to find the json, then to add to relationship index

        var cache = new List<IEntityRelationship>();
        var index = appState.Index;
        foreach (var entity in appState.List)
        {
            // find all attributes which are relationships
            var relationshipAttributes = entity.Attributes
                .Select(a => a.Value)
                .Where(a => a is IAttribute<IEnumerable<IEntity>>)
                .Cast<IAttribute<IEnumerable<IEntity>>>();

            // Keep only the ones which use a LazyEntitiesSource
            var lazyEntityValues = relationshipAttributes
                .Select(a => a.Typed.First().TypedContents as LazyEntitiesSource)
                .Where(tc => tc != null);

            foreach (var value in lazyEntityValues)
            foreach (var childId in value.EntityIds.Where(e => e != null))
                Add(entity, childId.Value);
        }

        return cache.ToImmutableList();



        // 2024-01-23 2dm - rewrote to the code below, must monitor for problems
        // Important changes
        // - directly add parent without lookup in index. It's not clear why it used the index, so if something pops up, we must document it
        // - removed the null-check for child, because it seems that the reason for it is a left-over
        //   since the nulls were filtered before
        void Add(IEntity parent, int childId)
        {
            if (index.TryGetValue(childId, out var child))
                cache.Add(new EntityRelationship(parent, child));
        }

    }
}