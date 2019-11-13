using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps
{
    [PrivateApi("don't publish this - too internal, special, complicated")]
    public class AppRelationshipManager: CacheChainedIEnumerable<EntityRelationship>
    {
        AppState App;
        public AppRelationshipManager(AppState upstream) : base(upstream, () => Rebuild(upstream))
        {
            App = upstream;
        }


        private static List<EntityRelationship> Rebuild(AppState appState)
        {
            // todo: could be optimized (minor)
            // atm guid-relationships (like in json-objects) 
            // will have multiple lookups - first to find the json, then to add to relationship index

            var cache = new List<EntityRelationship>();

            foreach (var entity in appState.List)
            foreach (var attrib in entity.Attributes.Select(a => a.Value)
                .Where(a => a is IAttribute<IEnumerable<IEntity>>)
                .Cast<IAttribute<IEnumerable<IEntity>>>())
            foreach (var val in ((LazyEntities)attrib.Typed[0].TypedContents).EntityIds.Where(e => e != null))
                Add(appState, cache, entity.EntityId, val);

            return cache;
        }

        [PrivateApi]
        public void AttachRelationshipResolver(IEntity entity)
        {
            foreach (var attrib in entity.Attributes.Select(a => a.Value)
                .Where(a => a is IAttribute<IEnumerable<IEntity>>)
                .Cast<IAttribute<IEnumerable<IEntity>>>()
            )
                (attrib.TypedContents as LazyEntities).AttachLookupList(App);
        }

        private static void Add(AppState appState, List<EntityRelationship> list, int parent, int? child)
        {
            var lookup = appState.Index;
            if (lookup.ContainsKey(parent) &&
                (!child.HasValue || lookup.ContainsKey(child.Value)))
                list.Add(new EntityRelationship(lookup[parent],
                    child.HasValue ? lookup[child.Value] : null));
        }
    }
}
