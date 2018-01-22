using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.App
{
    // todo: if we have time, optimize it so it doesn't do the lookup until accessed
    public class AppRelationshipManager: IEnumerable<EntityRelationshipItem>
    {
        private readonly AppDataPackage _app;
        private List<EntityRelationshipItem> _cache;

        public AppRelationshipManager(AppDataPackage app)
        {
            _app = app;
        }

        public void Add( int parent, int? child)
        {            
            //try
            //{
            var lookup = _app.Index;
            if (lookup.ContainsKey(parent) &&
                (!child.HasValue || lookup.ContainsKey(child.Value)))
                _cache.Add(new EntityRelationshipItem(lookup[parent],
                    child.HasValue ? lookup[child.Value] : null));
            //}
            //catch (KeyNotFoundException)
            //{
            //    /* ignore */
            //}
        }

        public List<EntityRelationshipItem> GetCache()
        {
             if (_cache != null) return _cache;


            // todo: could be optimized (minor)
            // atm guid-relationships (like in json-objects) 
            // will have multiple lookups - first to find the json, then to add to relationship index

            _cache = new List<EntityRelationshipItem>();

            foreach (var entity in _app.List)
            foreach (var attrib in entity.Attributes.Select(a => a.Value)
                .Where(a => a is IAttribute<EntityRelationship>)
                .Cast<IAttribute<EntityRelationship>>())
            foreach (var val in attrib.Typed[0].TypedContents.EntityIds.Where(e => e != null))
                Add(entity.EntityId, val);

            return _cache;
        }

        internal void Reset() => _cache = null;

        public IEnumerator<EntityRelationshipItem> GetEnumerator() 
            => GetCache().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
