using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight
{
    public partial class ConvertToEavLight
    {

        private void AddIdAndGuid(IEntityLight entity, IDictionary<string, object> entityValues, EntitySerializationDecorator rules)
        {
            // Add Id and Guid
            // ...only if these are not already existing with this name in the entity itself as an internal value
            if (rules?.SerializeId ?? true)
            {
                if (entityValues.ContainsKey(Attributes.IdNiceName)) entityValues.Remove(Attributes.IdNiceName);
                entityValues.Add(Attributes.IdNiceName, entity.EntityId);
            }

            // if rules.SerializeGuid are not set, then respect WithGuid
            // otherwise the rules should be applied, but default to false
            if (rules?.SerializeGuid == null && WithGuid || (rules?.SerializeGuid ?? false))
            {
                if (entityValues.ContainsKey(Attributes.GuidNiceName)) entityValues.Remove(Attributes.GuidNiceName);
                entityValues.Add(Attributes.GuidNiceName, entity.EntityGuid);
            }
        }


        private static void AddDateInformation(IEntityLight entity, IDictionary<string, object> entityValues, EntitySerializationDecorator rules)
        {
            // Include modified field, if there is not already one in the dictionary
            if (rules?.SerializeModified ?? true)
                if (!entityValues.ContainsKey(Attributes.ModifiedNiceName))
                    entityValues.Add(Attributes.ModifiedNiceName, entity.Modified);

            // Include created field, if there is not already one in the dictionary
            if (rules?.SerializeCreated ?? true)
                if (!entityValues.ContainsKey(Attributes.CreatedNiceName))
                    entityValues.Add(Attributes.CreatedNiceName, entity.Created);
        }

    }
}
