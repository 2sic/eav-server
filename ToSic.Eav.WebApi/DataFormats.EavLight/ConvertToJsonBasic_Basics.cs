using ToSic.Eav.Data.EntityDecorators.Sys;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.DataFormats.EavLight;

partial class ConvertToEavLight
{
    /// <summary>
    /// Add Id, Guid and possibly AppId (new v15).
    /// New v17: added withIdOverride and withGuidFallback
    /// </summary>
    private void AddAllIds(IEntity entity, IDictionary<string, object> entityValues, EntitySerializationDecorator rules, bool withGuidFallback)
    {
        // Id - on by default
        // ...only if these are not already existing with this name in the entity itself as an internal value
        if (rules.SerializeId ?? true) 
            AddOrReplaceValue(entityValues, AttributeNames.IdNiceName, entity.EntityId);

        if (rules.SerializeAppId ?? false)
            AddOrReplaceValue(entityValues, nameof(IAppIdentity.AppId), entity.AppId);

        // if rules.SerializeGuid are not set, then respect WithGuid
        // otherwise the rules should be applied, but default to false
        if (rules.SerializeGuid ?? withGuidFallback)
            AddOrReplaceValue(entityValues, AttributeNames.GuidNiceName, entity.EntityGuid);
    }

    private static void AddOrReplaceValue(IDictionary<string, object> entityValues, string fieldName, object value)
    {
        // Manually remove, because there could be differences in case-sensitivity
        entityValues.Remove(fieldName);
        entityValues.Add(fieldName, value);
    }


    private static void AddDateInformation(IEntity entity, IDictionary<string, object> entityValues, EntitySerializationDecorator rules)
    {
        // Include modified field, if there is not already one in the dictionary
        if (rules.SerializeModified ?? true)
            if (!entityValues.ContainsKey(AttributeNames.ModifiedNiceName))
                entityValues.Add(AttributeNames.ModifiedNiceName, entity.Modified);

        // Include created field, if there is not already one in the dictionary
        if (rules.SerializeCreated ?? true)
            if (!entityValues.ContainsKey(AttributeNames.CreatedNiceName))
                entityValues.Add(AttributeNames.CreatedNiceName, entity.Created);
    }

}