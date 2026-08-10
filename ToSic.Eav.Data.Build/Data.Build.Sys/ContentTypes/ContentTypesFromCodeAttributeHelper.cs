using System.Reflection;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.ContentTypes.Fields;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Values;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Helper to handle attribute information from PropertyInfos (reflection) for content types generated from code attributes.
/// </summary>
/// <param name="parentLog"></param>
internal class ContentTypesFromCodeAttributeHelper(ContentTypesFromCodeBuilder.Dependencies services, ILog parentLog)
    : HelperBase(parentLog, "CTC.AttHlp")
{
    private const int NoAppId = ContentTypesFromCodeBuilder.NoAppId;

    /// <summary>
    /// Main code
    /// </summary>
    internal (IList<IContentTypeField> attributes, IDecorator<IContentType>? additionalDecorators) Process(Type type)
    {
        var (fields, systemFields) = ExtractFields(type);

        // if we have system fields, generate a decorator
        IDecorator<IContentType>? sysFieldsDecorator = systemFields.SafeNone()
            ? null
            : new ContentTypeBuiltInAttributesDecorator(systemFields
                .ToDictionary(
                    va => va.Name,
                    va => va
                )
            );

        return (fields, sysFieldsDecorator);
    }


    private (IList<IContentTypeField> fields, IList<IContentTypeField> systemFields) ExtractFields(Type type)
    {
        var l = Log.Fn<(IList<IContentTypeField>, IList<IContentTypeField>)>(timer: true);

        // 1. Get all properties of the type; exit early if none
        var properties = type.GetProperties();

        if (properties.Length == 0)
            return l.Return(([], []), "no properties");

        // 2. Group by how it should be processed afterward
        var propsGrouped = properties
            .GroupBy(CategorizePropertyInfo)
            .ToListOpt();

        // 3. Handle normal / general properties
        var normalProps = propsGrouped
            .FirstOrDefault(g => g.Key == TempCategory.Normal)
            ?.ToListOpt();

        var normalFields = normalProps.SafeNone()
            ? []
            : PropertiesToFields(normalProps, keepEvenIfNoAttribute: true);

        // 4. Generate list of virtual attributes
        var systemProps = propsGrouped
            .FirstOrDefault(g => g.Key == TempCategory.System)
            ?.ToListOpt();
        var systemFields = systemProps.SafeNone()
            ? []
            : PropertiesToFields(systemProps, keepEvenIfNoAttribute: false);

        // Return everything
        return l.Return((normalFields, systemFields), $"real: {normalFields.Count}, virtual: {systemFields.Count}");




        static TempCategory CategorizePropertyInfo(PropertyInfo p) => p.Name switch
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

            _ => p.GetCustomAttribute<ContentTypeIgnoreAttribute>() != null
                ? TempCategory.Ignore
                : TempCategory.Normal,
        };
    }

    private enum TempCategory
    {
        Normal,
        System,
        Ignore
    }


    private IList<IContentTypeField> PropertiesToFields(IList<PropertyInfo> propsFiltered, bool keepEvenIfNoAttribute)
    {
        var pairs = propsFiltered
            .Select(p =>
                new
                {
                    PropertyInfo = p,
                    Specs = p.GetCustomAttributes<ContentTypeFieldAttribute>().FirstOrDefault(),
                })
            .Where(pair => pair.Specs != null || keepEvenIfNoAttribute)
            .ToListOpt();

        var fields = pairs
            .Select(pair =>
            {
                var specs = pair.Specs;
                var attrName = specs?.Name ?? pair.PropertyInfo.Name;
                var attrType = specs is null || specs.Type == ValueTypes.Undefined
                    ? ValueTypeHelpers.Get(pair.PropertyInfo.PropertyType)
                    : specs.Type;
                var attrIsTitle = specs?.IsTitle ?? false;

                // Create list of metadata with description; must be null if no metadata
                var mdItems = FieldSettingsGeneralMinimal
                    .FromCodeAttributeOrNull(specs)
                    .NullOrGetWith(mdRaw => services.DataFactory.Create([mdRaw]))
                    ?.ToListOpt();

                return services.TypeAssemblyKit.Field.Create(
                    NoAppId,
                    name: attrName,
                    type: attrType,
                    isTitle: attrIsTitle,
                    metadataItems: mdItems
                );
            })
            .ToListOpt();
        return fields;
    }

}
