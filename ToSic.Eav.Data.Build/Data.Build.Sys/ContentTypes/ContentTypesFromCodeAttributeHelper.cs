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
/// Extracts [ContentTypeField] attribute information from PropertyInfos (reflection).
/// For content types generated from code attributes.
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

        // 3. Normal properties
        var normalFields = PropertiesToFields(propsGrouped, TempCategory.Normal);

        // 4. System properties
        var systemFields = PropertiesToFields(propsGrouped, TempCategory.System);

        // Return everything
        return l.Return((normalFields, systemFields), $"real: {normalFields.Count}, virtual: {systemFields.Count}");
    }

    #region Categorize property infos by usage

    private enum TempCategory
    {
        Normal,
        System,
        Ignore
    }

    private static TempCategory CategorizePropertyInfo(PropertyInfo p) => p.Name switch
    {
        // System attributes
        AttributeNames.IdNiceName
            or AttributeNames.GuidNiceName
            or AttributeNames.CreatedNiceName
            or AttributeNames.ModifiedNiceName
            => TempCategory.System,

        // Standard built-in properties which are almost certainly never used otherwise
        nameof(IHasMetadata.Metadata)
            or nameof(IRelationshipKeys.RelationshipKeys)
            => TempCategory.Ignore,

        // Property "Values" which could be a real property, or the dictionary containing the values
        // note that Dictionary type check must be exact, not "IsAssignableFrom", otherwise it could have too many false positives
        nameof(IRawEntity.Values)
            when typeof(IDictionary<string, object?>) == p.PropertyType
            => TempCategory.Ignore,

        // Rest: Check if it's marked with a [ContentTypeIgnore] attribute
        _ => p.GetCustomAttribute<ContentTypeIgnoreAttribute>() != null
            ? TempCategory.Ignore
            : TempCategory.Normal,
    };
    
    #endregion


    private IList<IContentTypeField> PropertiesToFields(IList<IGrouping<TempCategory, PropertyInfo>> all, TempCategory category)
    {
        // Find all of category, exit early
        var ofCategory = all
            .FirstOrDefault(g => g.Key == category)
            ?.ToListOpt();
        if (ofCategory.SafeNone())
            return [];

        // Pair with the matching [ContentTypeField] attributes, will also find the [ContentTypeTitle] attributes, since it's inherited
        var pairs = ofCategory
            .Select(p =>
                new
                {
                    PropertyInfo = p,
                    Specs = p.GetCustomAttributes<ContentTypeFieldAttribute>().FirstOrDefault(),
                })
            // For "System", only keep the ones with attributes; otherwise keep all
            .Where(pair => category != TempCategory.System || pair.Specs != null)
            .ToListOpt();

        var fields = pairs
            .Select(pair =>
            {
                // Prepare resulting values
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

                // Return the field definition
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
