using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.App
{
    public class AppRelationshipManager: CacheChainedIEnumerable<EntityRelationship>
    {
        AppDataPackage App;
        public AppRelationshipManager(AppDataPackage upstream) : base(upstream, () => Rebuild(upstream))
        {
            App = upstream;
        }


        private static List<EntityRelationship> Rebuild(AppDataPackage appDataPackage)
        {
            // todo: could be optimized (minor)
            // atm guid-relationships (like in json-objects) 
            // will have multiple lookups - first to find the json, then to add to relationship index

            var cache = new List<EntityRelationship>();

            foreach (var entity in appDataPackage.List)
            foreach (var attrib in entity.Attributes.Select(a => a.Value)
                .Where(a => a is IAttribute<IEnumerable<IEntity>>)
                .Cast<IAttribute<IEnumerable<IEntity>>>())
            foreach (var val in ((LazyEntities)attrib.Typed[0].TypedContents).EntityIds.Where(e => e != null))
                Add(appDataPackage, cache, entity.EntityId, val);

            return cache;
        }

        public void AttachRelationshipResolver(IEntity entity)
        {
            foreach (var attrib in entity.Attributes.Select(a => a.Value)
                .Where(a => a is IAttribute<IEnumerable<IEntity>>)
                .Cast<IAttribute<IEnumerable<IEntity>>>()
            )
                (attrib.TypedContents as LazyEntities).AttachLookupList(App);
        }

        public static void Add(AppDataPackage appDataPackage, List<EntityRelationship> list, int parent, int? child)
        {
            var lookup = appDataPackage.Index;
            if (lookup.ContainsKey(parent) &&
                (!child.HasValue || lookup.ContainsKey(child.Value)))
                list.Add(new EntityRelationship(lookup[parent],
                    child.HasValue ? lookup[child.Value] : null));
        }
    }
}
