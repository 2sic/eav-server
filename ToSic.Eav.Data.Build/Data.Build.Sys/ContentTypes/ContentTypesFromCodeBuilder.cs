using System.Reflection;
using ToSic.Eav.Data.AttributeDefinition.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.Values;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Special system to manage and to convert c# classes with their definitions/attributes into content types.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class ContentTypesFromCodeBuilder(ContentTypeAssemblyKit ctAssemblyKit, EntityAssembler entityAssembler, AttributeListAssembler attributeListAssembler)
    : ServiceBase("Eav.CtFact")
{
    // TODO: Should probably be something different...?
    public const int NoAppId = -1;
    public const string AnonymousTypeName = "AnonymousType";

    internal IContentType Generate(Type type, string? name = default, string? nameId = default, string? scope = default, int appId = NoAppId)
    {
        var l = Log.Fn<IContentType>(timer: true);

        // 1. Get the content type specs from the attribute, if any and process
        var ctSpecs = type.GetDirectlyAttachedAttribute<ContentTypeSpecsAttribute>();
        var ctName = name
                     ?? ctSpecs?.Name
                     ?? (type.IsAnonymous() ? AnonymousTypeName : type.Name);
        var ctNameId = nameId
                       ?? ctSpecs?.Guid.NullOrGetWith(g => Guid.TryParse(g, out var guid) ? guid.ToString() : null)
                       ?? Guid.Empty.ToString();
        var ctScope = scope
                      ?? ctSpecs?.Scope.NullIfNoValue()
                      ?? ScopeConstants.Default;

        // Must be null if no metadata, so that it would then assume empty list...?
        var ctMdItems = CreateCtDetailsMetadataEntity(ctSpecs?.Description).ToListOfOneOrNull();
        var ctMdSource = MetadataProvider.Create(ctMdItems);
        var ctMetadata = new ContentTypeMetadata(typeId: ctNameId, title: ctName, source: ctMdSource);

        var (attributes, vAttributes) = GenerateAttributes(type);

        IDecorator<IContentType>? vAttributeDecorator = vAttributes == null || vAttributes.Count == 0
            ? null
            : new ContentTypeBuiltInAttributesDecorator(vAttributes
                .ToDictionary(
                    va => va.Name,
                    va => va
                )
            );

        var contentType = ctAssemblyKit.Type.Create(
            appId,
            name: ctName,
            nameId: ctNameId,
            scope: ctScope,
            id: 0,
            metadata: ctMetadata,
            isDynamic: false, // set dynamic to false, as the attributes are known, this type can only have these attributes
            attributes: attributes,
            decorators: vAttributeDecorator.ToListOfOneOrNull(),
            repositoryType: ctSpecs == null
                ? RepositoryTypes.CodeReflection
                : RepositoryTypes.CodeConfiguration
        );
        return l.ReturnAndLog(contentType);
    }

    /// <summary>
    /// Generate a details entity for a content type.
    /// Most properties like icon etc. are not important, so ATM it only does:
    /// - Description
    /// </summary>
    private IEntity? CreateCtDetailsMetadataEntity(string? description)
    {
        var l = Log.Fn<IEntity>();
        if (description == null)
            return l.ReturnNull("no description");

        // All props
        var dic = new Dictionary<string, object?>
        {
            { nameof(ContentTypeDetails.Description), description }
        };
        var attributes = attributeListAssembler.Finalize(dic);

        // Create a Description entity
        var entity = entityAssembler.Create(
            NoAppId,
            ctAssemblyKit.Type.Transient(NoAppId, ContentTypeDetails.ContentTypeName),
            attributes: attributes
        );
        return l.Return(entity, "created");
    }

    private (IList<IContentTypeAttribute> attributes, IList<IContentTypeAttribute>? vAttributes) GenerateAttributes(Type type)
    {
        var l = Log.Fn<(IList<IContentTypeAttribute>, IList<IContentTypeAttribute>?)>(timer: true);
        
        // 1. Get all properties of the type; exit early if none
        var properties = type.GetProperties();

        if (properties.Length == 0)
            return l.Return(([], null), "no properties");

        // 2. Group by how it should be processed afterward
        var propsGrouped = properties
            .GroupBy(p => IsSystemProperty(p)
                ? TempCategory.System
                : IsIgnoreProperty(p)
                    ? TempCategory.Ignore
                    : TempCategory.General
            )
            .ToListOpt();

        // 3. Handle normal / general properties
        var gDefault = propsGrouped
            .FirstOrDefault(g => g.Key == TempCategory.General)
            ?.ToListOpt();

        var attributes = gDefault == null || !gDefault.Any()
            ? []
            : PropertiesToAttributes(gDefault, false);

        // 4. Generate list of virtual attributes
        var gSystem = propsGrouped
            .FirstOrDefault(g => g.Key == TempCategory.System)
            ?.ToListOpt();
        var vAttributes = gSystem == null || !gSystem.Any()
            ? null
            : PropertiesToAttributes(gSystem, true);

        // Return everything
        return l.Return((attributes, vAttributes), $"real: {attributes.Count}, virtual: {vAttributes?.Count}");



        static bool IsSystemProperty(PropertyInfo p) =>
            p.Name is AttributeNames.IdNiceName
                or AttributeNames.GuidNiceName
                or AttributeNames.CreatedNiceName
                or AttributeNames.ModifiedNiceName;

        static bool IsIgnoreProperty(PropertyInfo p) =>
            p.Name switch
            {
                // Standard built-in properties which are almost certainly never used otherwise
                nameof(IHasMetadata.Metadata)
                    or nameof(IRelationshipKeys.RelationshipKeys)
                    => true,

                // Values property. which could be used otherwise as well, so we'll only skip if it's the 
                nameof(IRawEntity.Values)
                    //when typeof(IDictionary<string, object?>).IsAssignableFrom(p.PropertyType)
                    when typeof(IDictionary<string, object?>) == p.PropertyType
                    => true,
                _ => p.GetCustomAttribute<ContentTypeAttributeIgnoreAttribute>() != null
            };
    }

    private enum TempCategory
    {
        General,
        System,
        Ignore
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
                var specs = pair.Specs;
                var propertyInfo = pair.Property;
                var attrName = specs?.Name ?? propertyInfo.Name;
                var attrType = specs == null || specs.Type == ValueTypes.Undefined
                    ? ValueTypeHelpers.Get(propertyInfo.PropertyType)
                    : specs.Type;
                var attrIsTitle = specs?.IsTitle ?? false;

                // Must be null if no metadata
                var attrMetadata = ContentTypeAttributeDetails(
                        ContentTypeAttributeAll.FromCodeAttributeOrNull(specs)
                    )
                    .ToListOfOneOrNull();

                return ctAssemblyKit.Attribute.Create(
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
    /// - InputType
    /// </summary>
    /// <remarks>
    /// I guess we could use the DataFactory here, but as of 2026-07 we're careful to
    /// not introduce that as a dependency here, since it could end up circling back.
    /// </remarks>
    private IEntity? ContentTypeAttributeDetails(ContentTypeAttributeAll? attrAll)
    {
        var l = Log.Fn<IEntity>();

        if (attrAll == null)
            return l.ReturnNull("no details");

        // All props
        var dic = attrAll.BuildValues();
        var attributes = attributeListAssembler.Finalize(dic);

        // Create a Description entity
        var entity = entityAssembler.Create(
            NoAppId,
            ctAssemblyKit.Type.Transient(NoAppId, AttributeMetadataConstants.TypeGeneral),
            attributes: attributes
        );
        return l.Return(entity, "created");
    }

}