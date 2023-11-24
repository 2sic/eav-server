using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

partial class ConvertToEavLight
{
    /// <summary>
    /// Add Id, Guid and possibly AppId (new v15)
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="entityValues"></param>
    /// <param name="rules"></param>
    private void AddAllIds(IEntityLight entity, IDictionary<string, object> entityValues, EntitySerializationDecorator rules)
    {
        // Id - on by default
        // ...only if these are not already existing with this name in the entity itself as an internal value
        if (rules?.SerializeId ?? true) 
            AddOrReplaceValue(entityValues, Attributes.IdNiceName, entity.EntityId);

        if (rules?.SerializeAppId ?? false)
            AddOrReplaceValue(entityValues, nameof(IAppIdentity.AppId), entity.AppId);

        // Note: this doesn't work yet - we would have to lookup the ZoneId in the AppStates
        //if (rules?.SerializeZoneId ?? false)
        //    AddOrReplaceValue(entityValues, nameof(IAppIdentity.ZoneId), entity.AppId);

        // if rules.SerializeGuid are not set, then respect WithGuid
        // otherwise the rules should be applied, but default to false
        if (rules?.SerializeGuid == null && WithGuid || (rules?.SerializeGuid ?? false))
            AddOrReplaceValue(entityValues, Attributes.GuidNiceName, entity.EntityGuid);
    }

    private static void AddOrReplaceValue(IDictionary<string, object> entityValues, string fieldName, object value)
    {
        // Manually remove, because there could be differences in case-sensitivity
        if (entityValues.ContainsKey(fieldName)) entityValues.Remove(fieldName);
        entityValues.Add(fieldName, value);
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