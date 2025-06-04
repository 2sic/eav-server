using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Metadata;
using ToSic.Eav.Sys;
using ToSic.Lib.Coding;

namespace ToSic.Eav.Data.Build;

public static class BuildTestAccessors
{
    public static Entity CreateEntityTac(
        this DataBuilder dataBuilder,
        int appId,
        IContentType contentType,
        NoParamOrder noParamOrder = default,
        Dictionary<string, object>? values = default,
        Dictionary<string, IAttribute>? typedValues = default,
        int entityId = default,
        int repositoryId = EavConstants.NullId,
        Guid guid = default,
        string? titleField = default,
        DateTime? created = default,
        DateTime? modified = default,
        string? owner = default,
        int version = default,
        bool isPublished = true,
        ITarget? metadataFor = default,
        EntityPartsLazy? partsBuilder = default
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

    //public static IContentType CreateContentTypeTac(this ContentTypeBuilder builder,
    //    int appId,
    //    string name,
    //    int? id = default,
    //    string nameId = default,
    //    string scope = default,
    //    IList<IContentTypeAttribute> attributes = default)
    //{
    //    return builder.Create(appId: appId,
    //        id: id ?? 0,
    //        name: name,
    //        nameId: nameId,
    //        scope: scope ?? "TestScope",
    //        attributes: attributes);
    //}

    //public static IAttribute CreateTypedAttributeTac(this AttributeBuilder builder, string name, ValueTypes type, IList<IValue> values = null)
    //    => builder.Create(name, type, values);
}