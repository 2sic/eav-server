using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Metadata;
using ToSic.Lib.Coding;

namespace ToSic.Eav.Core.Tests;

public static class DataTestExtensions
{
    public static Entity CreateEntityTac(
        this DataBuilder dataBuilder,
        int appId,
        IContentType contentType,
        NoParamOrder noParamOrder = default,
        Dictionary<string, object> values = default,
        Dictionary<string, IAttribute> typedValues = default,
        int entityId = default,
        int repositoryId = Constants.NullId,
        Guid guid = default,
        string titleField = default,
        DateTime? created = default,
        DateTime? modified = default,
        string owner = default,
        int version = default,
        bool isPublished = true,
        ITarget metadataFor = default,
        EntityPartsBuilder partsBuilder = default
    )
    {
        return dataBuilder.Entity.Create(appId: appId,
            contentType: contentType,
            attributes: typedValues != null
                ? dataBuilder.Attribute.Create(typedValues)
                : values != null
                    ? dataBuilder.Attribute.Create(values)
                    : null,
            entityId: entityId,
            repositoryId: repositoryId,
            guid: guid,
            titleField: titleField,
            created: created, modified: modified,
            owner: owner,
            version: version,
            isPublished: isPublished,
            metadataFor: metadataFor,
            partsBuilder: partsBuilder

        );

    }

    public static IContentType CreateContentTypeTac(this ContentTypeBuilder builder,
        int appId,
        string name,
        int? id = default,
        string nameId = default,
        string scope = default,
        IList<IContentTypeAttribute> attributes = default)
    {
        return builder.Create(appId: appId,
            id: id ?? 0,
            name: name,
            nameId: nameId,
            scope: scope ?? "TestScope",
            attributes: attributes);
    }

    public static IAttribute CreateTypedAttributeTac(this AttributeBuilder builder, string name, ValueTypes type, IList<IValue> values = null)
        => builder.Create(name, type, values);
}