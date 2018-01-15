using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    public class AppRelationshipManager: List<EntityRelationshipItem>
    {
        public void Add(Dictionary<int, IEntity> entities, int parent, int? child)
        {
            if (entities.ContainsKey(parent) &&
                (!child.HasValue || entities.ContainsKey(child.Value)))
                Add(new EntityRelationshipItem(entities[parent],
                    child.HasValue ? entities[child.Value] : null));
        }
    }
}
