﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight
{
    public partial class ConvertToEavLight
    {
        private void AddMetadataAndFor(IEntity entity, IDictionary<string, object> entityValues, IEntitySerialization rules)
        {
            // if rules.MetadataFor are not set, then respect WithMetadataFor
            // otherwise the rules should be applied, but default to false
            var itemForRules = MetadataForSerialization.Stabilize(rules?.SerializeMetadataFor, null);
            if (itemForRules.Serialize ?? MetadataFor.Serialize ?? false)
                if (entity.MetadataFor.IsMetadata)
                    entityValues.Add(JsonKeyMetadataFor, entity.MetadataFor);

            var itemMdRules = SubEntitySerialization.Stabilize(rules?.SerializeMetadata, Metadata);
            
            if (itemMdRules.Serialize != true || !entity.Metadata.Any()) return;

            entityValues.Add(JsonKeyMetadata, CreateListOfSubEntities(entity.Metadata, itemMdRules));
        }


    }
}
