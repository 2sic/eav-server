using System.Reflection;
using ToSic.Eav.Data.ContentTypes.CodeAttributes;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build;

/// <summary>
/// Special factory to convert POCOs into content types.
/// </summary>
public class ContentTypeFactory(ContentTypeBuilder ctBuilder, ContentTypeAttributeBuilder ctAttributeBuilder, EntityBuilder entityBuilder, AttributeBuilder attributeBuilder)
    : ServiceBase("Eav.CtFact")
{
    // TODO: Should probably be something different...?
    public const int NoAppId = -1;

    public IContentType Create(Type type)
    {
        if (Cache.TryGetValue(type, out var contentType))
            return contentType;
        var created = Create(type, null, null, null);
        Cache[type] = created;
        return created;
    }

    private static readonly Dictionary<Type, IContentType> Cache = new();

    private IContentType Create(Type type, string name = default, string nameId = default, string scope = default, int appId = NoAppId)
    {
        var l = Log.Fn<IContentType>(timer: true);
        var ctSpecs = type.GetDirectlyAttachedAttribute<ContentTypeSpecsAttribute>();
        var ctName = name ?? ctSpecs?.Name ?? type.Name;
        var ctNameId = nameId ?? ctSpecs?.Guid.NullOrGetWith(g => Guid.TryParse(g, out var guid) ? guid.ToString() : null) ?? Guid.Empty.ToString();
        var ctScope = scope ?? ctSpecs?.Scope ?? Scopes.Default;

        // Must be null if no metadata
        var ctMetadata = ContentTypeDetails(ctSpecs?.Description)?.ToListOfOne();

        var attributes = GenerateAttributes(type);

        var contentType = ctBuilder.Create(
            appId,
            name: ctName,
            nameId: ctNameId,
            scope: ctScope,
            id: 0,
            metadataItems: ctMetadata,
            isDynamic: true,
            attributes: attributes
        );
        return l.ReturnAndLog(contentType);
    }

    /// <summary>
    /// Generate a details entity for a content type.
    /// Most properties like icon etc. are not important, so ATM it only does:
    /// - Description
    /// </summary>
    private IEntity ContentTypeDetails(string description)
    {
        var l = Log.Fn<IEntity>();
        if (description == null)
            return l.ReturnNull("no description");

        // All props
        var dic = new Dictionary<string, object> { { nameof(Data.ContentTypeDetails.Description), description } };
        var attributes = attributeBuilder.Create(dic);

        // Create a Description entity
        var entity = entityBuilder.Create(NoAppId, ctBuilder.Transient(NoAppId, Data.ContentTypeDetails.ContentTypeTypeName, Data.ContentTypeDetails.ContentTypeTypeName), attributes: attributes);
        return l.Return(entity, "created");
    }

    private List<IContentTypeAttribute> GenerateAttributes(Type type)
    {
        var l = Log.Fn<List<IContentTypeAttribute>>(timer: true);
        // Get all properties of the type
        var properties = type.GetProperties();

        if (properties.Length == 0)
            return l.Return([], "no properties");

        var propsFiltered = properties
            .Where(p =>
                p.Name is not (Attributes.IdNiceName or Attributes.GuidNiceName or Attributes.CreatedNiceName or Attributes.ModifiedNiceName)
                && p.GetCustomAttribute<ContentTypeAttributeIgnoreAttribute>() == null)
            .ToList();

        if (propsFiltered.Count == 0)
            return l.Return([], "no properties after filtering");

        // Generate list of attributes
        var attributes = propsFiltered.Select(p =>
            {
                var attrSpecs = p.GetCustomAttributes<ContentTypeAttributeSpecsAttribute>().FirstOrDefault();
                var attrName = attrSpecs?.Name ?? p.Name;
                var attrType = attrSpecs == null || attrSpecs.Type == ValueTypes.Undefined
                    ? ValueTypeHelpers.Get(p.PropertyType)
                    : attrSpecs.Type;
                var attrIsTitle = attrSpecs?.IsTitle ?? false;

                // Must be null if no metadata
                var attrMetadata = ContentTypeAttributeDetails(attrSpecs?.Description)?.ToListOfOne();

                return ctAttributeBuilder.Create(
                    NoAppId,
                    name: attrName,
                    type: attrType,
                    isTitle: attrIsTitle,
                    metadataItems: attrMetadata
                );
            })
            .ToList();

        return l.Return(attributes, $"{attributes.Count}");
    }

    /// <summary>
    /// Generate a details entity for an attribute of a content type.
    /// Most properties like icon etc. are not important, so ATM it only does:
    /// - Description
    /// </summary>
    private IEntity ContentTypeAttributeDetails(string description)
    {
        var l = Log.Fn<IEntity>();
        if (description == null)
            return l.ReturnNull("no description");

        // All props
        var dic = new Dictionary<string, object> { { AttributeMetadata.DescriptionField, description } };
        var attributes = attributeBuilder.Create(dic);

        // Create a Description entity
        var entity = entityBuilder.Create(NoAppId, ctBuilder.Transient(NoAppId, AttributeMetadata.TypeGeneral, AttributeMetadata.TypeGeneral), attributes: attributes);
        return l.Return(entity, "created");
    }

}