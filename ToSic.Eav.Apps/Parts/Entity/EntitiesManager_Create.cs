using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        public Tuple<int, Guid> Create(string typeName, Dictionary<string, object> values, ITarget metadataFor = null)
        {
            var wrapLog = Log.Fn<Tuple<int, Guid>>($"type:{typeName}, val-count:{values.Count}, meta:{metadataFor}");
            var newEnt = new Entity(Parent.AppId, Guid.NewGuid(), Parent.Read.ContentTypes.Get(typeName), values);
            if (metadataFor != null) newEnt.SetMetadata(metadataFor as Metadata.Target);
            var eid = Save(newEnt);
            var guid = Parent.DataController.Entities.TempLastSaveGuid;
            return wrapLog.Return(new Tuple<int, Guid>(eid, guid), $"id:{eid}, guid:{guid}");
        }

        
        /// <summary>
        /// Get an entity, or create it with the values provided.
        /// Important for use cases, where an information must exist for sure, so it would be created with the provided defaults
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="typeName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetOrCreate(Guid newGuid, string typeName, Dictionary<string, object> values)
        {
            Log.A($"get or create guid:{newGuid}, type:{typeName}, val-count:{values.Count}");
            if (Parent.DataController.Entities.EntityExists(newGuid))
            {
                // check if it's deleted - if yes, resurrect
                var existingEnt = Parent.DataController.Entities.GetEntitiesByGuid(newGuid).First();
                if (existingEnt.ChangeLogDeleted != null)
                    existingEnt.ChangeLogDeleted = null;

                return existingEnt.EntityId;
            }

            var newE = new Entity(Parent.AppId, newGuid, Parent.Read.ContentTypes.Get(typeName), values);
            return Save(newE);
        }
    }
}
