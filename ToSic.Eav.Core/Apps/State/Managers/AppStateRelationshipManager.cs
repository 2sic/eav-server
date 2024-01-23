using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Lib.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.State;

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
                // 2024-01-23 2dm - rewrote to the code below, must monitor for problems
                //Add(index, cache, entity.EntityId, childId);
                Add(entity/*.EntityId*/, childId.Value);
        }

        return cache.ToImmutableList();



        // 2024-01-23 2dm - rewrote to the code below, must monitor for problems
        // Important changes
        // - directly add parent without lookup in index. It's not clear why it used the index, so if something pops up, we must document it
        // - removed the null-check for child, because it seems that the reason for it is a left-over
        //   since the nulls were filtered before
        void Add(IEntity parent, int childId)
        {
            // 2024-01-23 2dm - new exit point id child == null, monitor till 2024-Q3
            if (index.TryGetValue(childId, out var child))
                cache.Add(new(parent, child));
        }

        #region Archive till #Remove2024-Q3

        //// 2024-01-23 2dm - rewrote to the code below, must monitor for problems
        //// Important changes
        //// - directly add parent without lookup in index. It's not clear why it used the index, so if something pops up, we must document it
        //// - removed the null-check for child, because it seems that the reason for it is a left-over
        ////   since the nulls were filtered before
        //void Add(int parentId, int/*?*/ childId)
        //{
        //    // See if we can find the parent; otherwise exit
        //    var parent = index.TryGetValue(parentId, out var p) ? p : null;
        //    if (parent == null) return;

        //    var child = index.TryGetValue(childId, out var c) ? c : null;
        //    cache.Add(new(parent, child));
        //}

        #endregion

    }

    #region Archive till #Remove2024-Q3

    //2024-01-23 2dm - rewrote to the code above but must keep this till 2024-Q3
    //in case something breaks

    //private static void Add(IReadOnlyDictionary<int, IEntity> lookup, List<EntityRelationship> list, int parent, int? child)
    //{
    //    //var lookup = appState.Index;
    //    if (lookup.ContainsKey(parent) &&
    //        (!child.HasValue || lookup.ContainsKey(child.Value)))
    //        list.Add(new(lookup[parent], child.HasValue ? lookup[child.Value] : null));
    //}

    #endregion
}