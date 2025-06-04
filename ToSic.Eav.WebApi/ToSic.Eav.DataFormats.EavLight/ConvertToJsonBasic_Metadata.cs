using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

partial class ConvertToEavLight
{
    private void AddMetadataAndFor(IEntity entity, IDictionary<string, object> entityValues, EntitySerializationDecorator rules)
    {
        // if rules.MetadataFor are not set, then respect WithMetadataFor
        // otherwise the rules should be applied, but default to false
        var itemForRules = MetadataForSerialization.Stabilize(rules.SerializeMetadataFor, MetadataFor);
        if (itemForRules.Serialize /*?? MetadataFor.Serialize*/ ?? false)
            if (entity.MetadataFor.IsMetadata)
                entityValues.Add(AttributeNames.JsonKeyMetadataFor, entity.MetadataFor);
        
        var itemMdRules = SubEntitySerialization.Stabilize(rules.SerializeMetadata, Metadata);
            
        if (itemMdRules.Serialize != true || !entity.Metadata.Any()) return;

        entityValues.Add(AttributeNames.JsonKeyMetadata, SubConverter.CreateListOfSubEntities(entity.Metadata, itemMdRules));
    }


}