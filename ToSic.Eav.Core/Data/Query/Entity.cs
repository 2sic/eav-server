using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data.Query
{
    public static class Entity
    {
        public static IEntity One(this IEnumerable<IEntity> entities, int id) 
            => entities.FirstOrDefault(e => e.EntityId == id);

        public static IEntity One(this IEnumerable<IEntity> entities, Guid guid) 
            => entities.FirstOrDefault(e => e.EntityGuid == guid);

        public static bool Has(this IEnumerable<IEntity> entities, int id) 
            => entities.Any(e => e.EntityId == id);

        public static bool Has(this IEnumerable<IEntity> entities, Guid guid) 
            => entities.Any(e => e.EntityGuid == guid);


        public static IEnumerable<IEntity> Many(this IEnumerable<IEntity> entities, int id) 
            => entities.Where(e => e.EntityId == id);
    }
}
