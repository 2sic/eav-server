using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    public class AppRelationshipManager: List<EntityRelationshipItem>
    {
        private readonly Dictionary<int, IEntity> _lookup;

        public AppRelationshipManager(Dictionary<int, IEntity> entities) 
        {
            _lookup = entities;
        }

        public void Add(int parent, int? child)
        {            
            //try
            //{

            if (_lookup.ContainsKey(parent) &&
                (!child.HasValue || _lookup.ContainsKey(child.Value)))
                Add(new EntityRelationshipItem(_lookup[parent],
                    child.HasValue ? _lookup[child.Value] : null));
            //}
            //catch (KeyNotFoundException)
            //{
            //    /* ignore */
            //}
        }
    }
}
