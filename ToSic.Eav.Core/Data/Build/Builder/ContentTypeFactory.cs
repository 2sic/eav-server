using ToSic.Eav.Data.ContentTypes.CodeAttributes;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data.Build;

public class ContentTypeFactory(ContentTypeBuilder ctBuilder, ContentTypeAttributeBuilder ctAttributeBuilder, EntityBuilder entityBuilder, AttributeBuilder attributeBuilder)
{
    public const int NoAppId = -1;

    public IContentType Create(Type type) => Create(type, null, null, null);

    private IContentType Create(Type type, string name = default, string nameId = default, string scope = default, int appId = NoAppId)
    {
        var ctSpecs = type.GetDirectlyAttachedAttribute<ContentTypeSpecsAttribute>();
        var ctName = name ?? ctSpecs?.Name ?? type.Name;
        var ctNameId = nameId ?? ctSpecs?.Guid.NullOrGetWith(g => Guid.TryParse(g, out var guid) ? guid.ToString() : null) ?? Guid.Empty.ToString();
        var ctScope = scope ?? ctSpecs?.Scope ?? Scopes.Default;

        // Must be null if no metadata
        var ctMetadata = ContentTypeDetails(ctSpecs?.Description)?.ToListOfOne();

        var contentType = ctBuilder.Create(
            appId,
            name: ctName,
            nameId: ctNameId,
            scope: ctScope,
            id: 0,
            metadataItems: ctMetadata,
            isDynamic: true
        );
        return contentType;
    }

    /// <summary>
    /// Generate a details entity for a content type.
    /// Most properties like icon etc. are not important, so ATM it only does:
    /// - Description
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    private IEntity ContentTypeDetails(string description)
    {
        if (description == null)
            return null;

        // All props
        var dic = new Dictionary<string, object> { { nameof(Data.ContentTypeDetails.Description), description } };
        var attributes = attributeBuilder.Create(dic);

        // Create a Description entity
        var entity = entityBuilder.Create(NoAppId, ctBuilder.Transient(NoAppId, Data.ContentTypeDetails.ContentTypeTypeName, Data.ContentTypeDetails.ContentTypeTypeName), attributes: attributes);
        return entity;
    }

}