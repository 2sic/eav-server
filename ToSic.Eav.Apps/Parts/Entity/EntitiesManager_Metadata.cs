using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        // todo: should be in metadata manager?
        public void SaveMetadata(Target target, string typeName, Dictionary<string, object> values
        ) => Log.Do("target:" + target.KeyNumber + "/" + target.KeyGuid + ", values count:" + values.Count, () =>
        {
            if (target.TargetType != (int)TargetTypes.Attribute || target.KeyNumber == null || target.KeyNumber == 0)
                throw new NotSupportedException("atm this command only creates metadata for entities with id-keys");

            // see if a metadata already exists which we would update
            var existingEntity = Parent.AppState.List
                .FirstOrDefault(e => e.MetadataFor?.TargetType == target.TargetType && e.MetadataFor?.KeyNumber == target.KeyNumber);
            if (existingEntity != null)
                UpdateParts(existingEntity.EntityId, values);
            else
            {
                var saveEnt = Builder.Entity.Create(appId: Parent.AppId, guid: Guid.NewGuid(),
                    contentType: Parent.Read.ContentTypes.Get(typeName),
                    attributes: Builder.Attribute.Create(values),
                    metadataFor: target);
                //saveEnt.SetMetadata(target);
                Save(saveEnt);
            }
        });
    }
}
