using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    public partial class ConvertToJsonLight
    {
        private void AddMetadataAndFor(IEntity entity, IDictionary<string, object> entityValues, IEntitySerialization rules)
        {
            // if rules.MetadataFor are not set, then respect WithMetadataFor
            // otherwise the rules should be applied, but default to false
            if (rules?.SerializeMetadataFor == null && WithMetadataFor || (rules?.SerializeMetadataFor ?? false))
                if (entity.MetadataFor.IsMetadata)
                    entityValues.Add(JsonKeyMetadataFor, entity.MetadataFor);

            var mdRules = SubEntitySerialization.Stabilize(rules?.SerializeMetadata, false, false, false, false);
            
            if (mdRules.Serialize != true || !entity.Metadata.Any()) return;

            entityValues.Add(JsonKeyMetadata, CreateListOfSubEntities(entity.Metadata, mdRules));
        }


    }
}
