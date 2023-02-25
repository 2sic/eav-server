using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Core.Tests
{
    public static class DataTestExtensions
    {
        public static Entity TestCreate(this EntityBuilder entityBuilder,
            int appId,
            IContentType contentType,
            string noParamOrder = Eav.Parameters.Protector,
            Dictionary<string, object> values = default,
            Dictionary<string, IAttribute> typedValues = default,
            int entityId = default,
            int repositoryId = Constants.NullId,
            Guid guid = default,
            string titleField = default,
            DateTime? created = default, DateTime? modified = default,
            string owner = default,
            int version = default,
            bool isPublished = true,
            ITarget metadataFor = default,
            EntityPartsBuilder partsBuilder = default
            )
        {
            return entityBuilder.Create(appId: appId,
                contentType: contentType,
                values: values,
                typedValues: typedValues,
                entityId: entityId,
                repositoryId: repositoryId,
                guid:guid,
                titleField:titleField,
                created: created, modified: modified,
                owner: owner,
                version: version,
                isPublished: isPublished,
                metadataFor: metadataFor,
                partsBuilder: partsBuilder

            );

        }
    }
}
