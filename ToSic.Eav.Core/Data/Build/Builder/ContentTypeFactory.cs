using System.Reflection;
using ToSic.Eav.Data.Internal;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Internal.Generics;
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

        var (attributes, vAttributes) = GenerateAttributes(type);

        var vAttributeDecorator = vAttributes == null || vAttributes.Count == 0
            ? null
            : new ContentTypeVirtualAttributes(vAttributes.ToDictionary(va => va.Name, va => va))
                as IDecorator<IContentType>;

        var contentType = ctBuilder.Create(
            appId,
            name: ctName,
            nameId: ctNameId,
            scope: ctScope,
            id: 0,
            metadataItems: ctMetadata,
            isDynamic: true,
            attributes: attributes,
            decorators: vAttributeDecorator?.ToListOfOne()
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

    private (List<IContentTypeAttribute> attributes, List<IContentTypeAttribute> vAttributes) GenerateAttributes(Type type)
    {
        var l = Log.Fn<(List<IContentTypeAttribute> attributes, List<IContentTypeAttribute> vAttributes)>(timer: true);
        // Get all properties of the type
        var properties = type.GetProperties();

        if (properties.Length == 0)
            return l.Return(([], null), "no properties");

        var propsGrouped = properties
            .GroupBy(p =>
                p.Name is (Attributes.IdNiceName or Attributes.GuidNiceName or Attributes.CreatedNiceName or Attributes.ModifiedNiceName)
                    ? "system"
                    : p.GetCustomAttribute<ContentTypeAttributeIgnoreAttribute>() == null
                        ? "default"
                        : "ignore"
            )
            .ToList();

        var gDefault = propsGrouped.FirstOrDefault(g => g.Key == "default")?.ToList();

        // Generate list of attributes
        var attributes = gDefault == null || !gDefault.Any()
            ? []
            : PropertiesToAttributes(gDefault, false);

        // Generate list of virtual attributes
        var gSystem = propsGrouped.FirstOrDefault(g => g.Key == "system")?.ToList();
        var vAttributes = gSystem == null || !gSystem.Any()
            ? null
            : PropertiesToAttributes(gSystem, true);


        return l.Return((attributes, vAttributes), $"real: {attributes.Count}, virtual: {vAttributes?.Count}");
    }

    private List<IContentTypeAttribute> PropertiesToAttributes(List<PropertyInfo> propsFiltered, bool skipNoMetadata)
    {
        var pairs = propsFiltered
            .Select(p =>
                new
                {
                    Property = p,
                    Specs = p.GetCustomAttributes<ContentTypeAttributeSpecsAttribute>().FirstOrDefault(),
                })
            .Where(pair => !skipNoMetadata || pair.Specs != null)
            .ToList();

        var attributes = pairs.Select(pair =>
            {
                var specs = pair.Specs; //.GetCustomAttributes<ContentTypeAttributeSpecsAttribute>().FirstOrDefault();
                var props = pair.Property;
                var attrName = specs?.Name ?? props.Name;
                var attrType = specs == null || specs.Type == ValueTypes.Undefined
                    ? ValueTypeHelpers.Get(props.PropertyType)
                    : specs.Type;
                var attrIsTitle = specs?.IsTitle ?? false;

                // Must be null if no metadata
                var attrMetadata = ContentTypeAttributeDetails(specs?.Description, specs?.InputTypeWIP)
                    ?.ToListOfOne();

                return ctAttributeBuilder.Create(
                    NoAppId,
                    name: attrName,
                    type: attrType,
                    isTitle: attrIsTitle,
                    metadataItems: attrMetadata
                );
            })
            .ToList();
        return attributes;
    }

    /// <summary>
    /// Generate a details entity for an attribute of a content type.
    /// Most properties like icon etc. are not important, so ATM it only does:
    /// - Description
    /// </summary>
    #nullable enable   // Enables nullable annotations and warnings
    private IEntity ContentTypeAttributeDetails(string? description, string? inputType)
    {
        var l = Log.Fn<IEntity>();
        if (description == null && inputType == null)
            return l.ReturnNull("no description");

        // All props
        var dic = new Dictionary<string, object>();
        if (description != null)
            dic.Add(AttributeMetadata.DescriptionField, description);
        if (inputType != null)
            dic.Add(AttributeMetadata.GeneralFieldInputType, inputType);

        var attributes = attributeBuilder.Create(dic);

        // Create a Description entity
        var entity = entityBuilder.Create(NoAppId, ctBuilder.Transient(NoAppId, AttributeMetadata.TypeGeneral, AttributeMetadata.TypeGeneral), attributes: attributes);
        return l.Return(entity, "created");
    }
    #nullable restore  // Restores the project-level nullable setting

}