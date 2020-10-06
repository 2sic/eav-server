using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {

        public void SaveMetadata(Target target, string typeName, Dictionary<string, object> values)
        {
            var wrapLog = Log.Call("target:" + target.KeyNumber + "/" + target.KeyGuid + ", values count:" + values.Count);

            if (target.TargetType != Constants.MetadataForAttribute || target.KeyNumber == null || target.KeyNumber == 0)
                throw new NotImplementedException("atm this command only creates metadata for entities with id-keys");

            // see if a metadata already exists which we would update
            var existingEntity = AppManager.AppState.List
                .FirstOrDefault(e => e.MetadataFor?.TargetType == target.TargetType && e.MetadataFor?.KeyNumber == target.KeyNumber);
            if (existingEntity != null)
                UpdateParts(existingEntity.EntityId, values);
            else
            {
                var saveEnt = new Entity(AppManager.AppId, Guid.NewGuid(), AppManager.Read.ContentTypes.Get(typeName), values);
                saveEnt.SetMetadata(target);
                Save(saveEnt);
            }
            wrapLog("ok");
        }
    }
}
