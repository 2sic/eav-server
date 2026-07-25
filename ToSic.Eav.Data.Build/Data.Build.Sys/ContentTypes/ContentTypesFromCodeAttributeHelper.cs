using System.Configuration;
using System.Reflection;
using ToSic.Eav.Data.AttributeDefinition.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Values;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Helper to handle attribute information from PropertyInfos (reflection) for content types generated from code attributes.
/// </summary>
/// <param name="ctAssemblyKit"></param>
/// <param name="entityAssembler"></param>
/// <param name="attributeListAssembler"></param>
/// <param name="parentLog"></param>
internal class ContentTypesFromCodeAttributeHelper(
    ContentTypeAssemblyKit ctAssemblyKit,
    EntityAssembler entityAssembler,
    AttributeListAssembler attributeListAssembler,
    ILog parentLog)
    : HelperBase(parentLog, "CTC.AttHlp")
{
    private const int NoAppId = ContentTypesFromCodeBuilder.NoAppId;

    /// <summary>
    /// Main code
    /// </summary>
    internal (IList<IContentTypeAttribute> attributes, IDecorator<IContentType>? additionalDecorators) Process(Type type)
    {
        var (attributes, builtInAttributes) = GenerateAttributes(type);

        IDecorator<IContentType>? builtInAttributeDecorator =
            builtInAttributes == null || builtInAttributes.Count == 0
                ? null
                : new ContentTypeBuiltInAttributesDecorator(builtInAttributes
                    .ToDictionary(
                        va => va.Name,
                        va => va
                    )
                );

        return (attributes, builtInAttributeDecorator);
    }


    private (IList<IContentTypeAttribute> attributes, IList<IContentTypeAttribute>? builtInAttributes) GenerateAttributes(Type type)
    {
        var l = Log.Fn<(IList<IContentTypeAttribute>, IList<IContentTypeAttribute>?)>(timer: true);

        // 1. Get all properties of the type; exit early if none
        var properties = type.GetProperties();

        if (properties.Length == 0)
            return l.Return(([], null), "no properties");

        // 2. Group by how it should be processed afterward
        var propsGrouped = properties
            .GroupBy(CategorizePropertyInfo)
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




        static TempCategory CategorizePropertyInfo(PropertyInfo p) =>
            p.Name switch
            {
                AttributeNames.IdNiceName
                    or AttributeNames.GuidNiceName
                    or AttributeNames.CreatedNiceName
                    or AttributeNames.ModifiedNiceName
                    => TempCategory.System,

                // Standard built-in properties which are almost certainly never used otherwise
                nameof(IHasMetadata.Metadata)
                    or nameof(IRelationshipKeys.RelationshipKeys)
                    => TempCategory.Ignore,

                // Values property. which could be used otherwise as well, so we'll only skip if it's the 
                nameof(IRawEntity.Values)
                    //when typeof(IDictionary<string, object?>).IsAssignableFrom(p.PropertyType)
                    when typeof(IDictionary<string, object?>) == p.PropertyType
                    => TempCategory.Ignore,

                _ => p.GetCustomAttribute<ContentTypeAttributeIgnoreAttribute>() != null
                    ? TempCategory.Ignore
                    : TempCategory.General,
            };

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
