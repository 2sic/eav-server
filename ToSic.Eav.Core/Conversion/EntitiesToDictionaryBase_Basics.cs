﻿using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Conversion
{
    public abstract partial class EntitiesToDictionaryBase
    {

        private void AddIdAndGuid(IEntity entity, Dictionary<string, object> entityValues, IEntitySerialization rules)
        {
            // Add Id and Guid
            // ...only if these are not already existing with this name in the entity itself as an internal value
            if (rules?.SerializeId ?? true)
            {
                if (entityValues.ContainsKey(IdField)) entityValues.Remove(IdField);
                entityValues.Add(IdField, entity.EntityId);
            }

            // if rules.SerializeGuid are not set, then respect WithGuid
            // otherwise the rules should be applied, but default to false
            if (rules?.SerializeGuid == null && WithGuid || (rules?.SerializeGuid ?? false))
            {
                if (entityValues.ContainsKey(Constants.SysFieldGuid)) entityValues.Remove(Constants.SysFieldGuid);
                entityValues.Add(Constants.SysFieldGuid, entity.EntityGuid);
            }
        }


        private static void AddDateInformation(IEntity entity, Dictionary<string, object> entityValues, IEntitySerialization rules)
        {
            // Include modified field, if there is not already one in the dictionary
            if (rules?.SerializeModified ?? true)
                if (!entityValues.ContainsKey(Constants.SysFieldModified))
                    entityValues.Add(Constants.SysFieldModified, entity.Modified);

            // Include created field, if there is not already one in the dictionary
            if (rules?.SerializeCreated ?? true)
                if (!entityValues.ContainsKey(Constants.SysFieldCreated))
                    entityValues.Add(Constants.SysFieldCreated, entity.Created);
        }

    }
}
