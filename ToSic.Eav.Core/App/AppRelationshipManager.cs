using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.App
{
    public class AppRelationshipManager: CacheChainedIEnumerable<EntityRelationshipItem>
    {
        AppDataPackage App;
        public AppRelationshipManager(AppDataPackage upstream) : base(upstream, () => Rebuild(upstream))
        {
            App = upstream;
        }


        private static List<EntityRelationshipItem> Rebuild(AppDataPackage appDataPackage)
        {
            // todo: could be optimized (minor)
            // atm guid-relationships (like in json-objects) 
            // will have multiple lookups - first to find the json, then to add to relationship index

            var cache = new List<EntityRelationshipItem>();

            foreach (var entity in appDataPackage.List)
            foreach (var attrib in entity.Attributes.Select(a => a.Value)
                .Where(a => a is IAttribute<LazyEntities>)
                .Cast<IAttribute<LazyEntities>>())
            foreach (var val in attrib.Typed[0].TypedContents.EntityIds.Where(e => e != null))
                Add(appDataPackage, cache, entity.EntityId, val);

            return cache;
        }

        public void AttachRelationshipResolver(IEntity entity)
        {
            foreach (var attrib in entity.Attributes.Select(a => a.Value)
                .Where(a => a is IAttribute<LazyEntities>)
                .Cast<IAttribute<LazyEntities>>()
            )
                attrib.TypedContents.AttachLookupList(App);
        }

        public static void Add(AppDataPackage appDataPackage, List<EntityRelationshipItem> list, int parent, int? child)
        {
            var lookup = appDataPackage.Index;
            if (lookup.ContainsKey(parent) &&
                (!child.HasValue || lookup.ContainsKey(child.Value)))
                list.Add(new EntityRelationshipItem(lookup[parent],
                    child.HasValue ? lookup[child.Value] : null));
        }
    }
}
