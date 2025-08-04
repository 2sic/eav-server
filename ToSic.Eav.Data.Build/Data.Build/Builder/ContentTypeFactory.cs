﻿using System.Reflection;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Values;

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
        var created = Create(type,  name: null, nameId: null, scope: null);
        Cache[type] = created;
        return created;
    }

    private static readonly Dictionary<Type, IContentType> Cache = new();

    // ReSharper disable once MethodOverloadWithOptionalParameter
    private IContentType Create(Type type, string? name = default, string? nameId = default, string? scope = default, int appId = NoAppId)
    {
        var l = Log.Fn<IContentType>(timer: true);
        var ctSpecs = type.GetDirectlyAttachedAttribute<ContentTypeSpecsAttribute>();
        var ctName = name ?? ctSpecs?.Name ?? type.Name;
        var ctNameId = nameId
                       ?? ctSpecs?.Guid.NullOrGetWith(g => Guid.TryParse(g, out var guid) ? guid.ToString() : null!)
                       ?? Guid.Empty.ToString();
        var ctScope = scope ?? ctSpecs?.Scope.NullIfNoValue() ?? ScopeConstants.Default;

        // Must be null if no metadata
        var ctMetadata = ContentTypeDetails(ctSpecs?.Description).ToListOfOneOrNull();

        var (attributes, vAttributes) = GenerateAttributes(type);

        IDecorator<IContentType>? vAttributeDecorator = vAttributes == null || vAttributes.Count == 0
            ? null
            : new ContentTypeVirtualAttributes(vAttributes.ToDictionary(va => va.Name, va => va));

        var contentType = ctBuilder.Create(
            appId,
            name: ctName,
            nameId: ctNameId,
            scope: ctScope,
            id: 0,
            metadataItems: ctMetadata,
            isDynamic: true,
            attributes: attributes,
            decorators: vAttributeDecorator.ToListOfOneOrNull()
        );
        return l.ReturnAndLog(contentType);
    }

    /// <summary>
    /// Generate a details entity for a content type.
    /// Most properties like icon etc. are not important, so ATM it only does:
    /// - Description
    /// </summary>
    private IEntity? ContentTypeDetails(string? description)
    {
        var l = Log.Fn<IEntity>();
        if (description == null)
            return l.ReturnNull("no description");

        // All props
        var dic = new Dictionary<string, object?> { { nameof(Sys.ContentTypes.ContentTypeDetails.Description), description } };
        var attributes = attributeBuilder.Create(dic);

        // Create a Description entity
        var entity = entityBuilder.Create(NoAppId, ctBuilder.Transient(NoAppId, Sys.ContentTypes.ContentTypeDetails.ContentTypeTypeName, Sys.ContentTypes.ContentTypeDetails.ContentTypeTypeName), attributes: attributes);
        return l.Return(entity, "created");
    }

    private (IList<IContentTypeAttribute> attributes, IList<IContentTypeAttribute>? vAttributes) GenerateAttributes(Type type)
    {
        var l = Log.Fn<(IList<IContentTypeAttribute>, IList<IContentTypeAttribute>?)>(timer: true);
        // Get all properties of the type
        var properties = type.GetProperties();

        if (properties.Length == 0)
            return l.Return(([], null), "no properties");

        var propsGrouped = properties
            .GroupBy(p =>
                p.Name is (AttributeNames.IdNiceName or AttributeNames.GuidNiceName or AttributeNames.CreatedNiceName or AttributeNames.ModifiedNiceName)
                    ? "system"
                    : p.GetCustomAttribute<ContentTypeAttributeIgnoreAttribute>() == null
                        ? "default"
                        : "ignore"
            )
            .ToListOpt();

        var gDefault = propsGrouped
            .FirstOrDefault(g => g.Key == "default")
            ?.ToListOpt();

        // Generate list of attributes
        var attributes = gDefault == null || !gDefault.Any()
            ? []
            : PropertiesToAttributes(gDefault, false);

        // Generate list of virtual attributes
        var gSystem = propsGrouped
            .FirstOrDefault(g => g.Key == "system")
            ?.ToListOpt();
        var vAttributes = gSystem == null || !gSystem.Any()
            ? null
            : PropertiesToAttributes(gSystem, true);


        return l.Return((attributes, vAttributes), $"real: {attributes.Count}, virtual: {vAttributes?.Count}");
    }

    private IList<IContentTypeAttribute> PropertiesToAttributes(IList<PropertyInfo> propsFiltered, bool skipNoMetadata)
    {
        var pairs = propsFiltered
            .Select(p =>
                new
                {
                    Property = p,
                    Specs = p.GetCustomAttributes<ContentTypeAttributeSpecsAttribute>().FirstOrDefault(),
                })
            .Where(pair => !skipNoMetadata || pair.Specs != null)
            .ToListOpt();

        var attributes = pairs
            .Select(pair =>
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
                    .ToListOfOneOrNull();

                return ctAttributeBuilder.Create(
                    NoAppId,
                    name: attrName,
                    type: attrType,
                    isTitle: attrIsTitle,
                    metadataItems: attrMetadata
                );
            })
            .ToListOpt();
        return attributes;
    }

    /// <summary>
    /// Generate a details entity for an attribute of a content type.
    /// Most properties like icon etc. are not important, so ATM it only does:
    /// - Description
    /// </summary>
    private IEntity? ContentTypeAttributeDetails(string? description, string? inputType)
    {
        var l = Log.Fn<IEntity>();
        if (description == null && inputType == null)
            return l.ReturnNull("no description");

        // All props
        var dic = new Dictionary<string, object?>();
        if (description != null)
            dic.Add(AttributeMetadataConstants.DescriptionField, description);
        if (inputType != null)
            dic.Add(AttributeMetadataConstants.GeneralFieldInputType, inputType);

        var attributes = attributeBuilder.Create(dic);

        // Create a Description entity
        var entity = entityBuilder.Create(
            NoAppId,
            ctBuilder.Transient(NoAppId, AttributeMetadataConstants.TypeGeneral, AttributeMetadataConstants.TypeGeneral),
            attributes: attributes);
        return l.Return(entity, "created");
    }

}