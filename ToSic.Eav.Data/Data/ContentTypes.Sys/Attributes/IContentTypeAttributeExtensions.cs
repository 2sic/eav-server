using ToSic.Eav.Data.Attributes.Sys;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Eav.Data.Sys;
using static ToSic.Eav.Data.Attributes.Sys.AttributeMetadataConstants;

namespace ToSic.Eav.Data.ContentTypes.Sys;

[PrivateApi]
// ReSharper disable once InconsistentNaming
public static class IContentTypeAttributeExtensions
{
    /// <summary>
    /// Will look up the input type name of an attribute. It uses a series of cascades, because historically this can be missing or written in a different way.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// It's important to NOT cache this result, because it can change during runtime, and then a cached info would be wrong. 
    /// </remarks>
    public static string InputType(this IContentTypeAttribute definition)
    {
        // Preferred storage and available in all fields defined after 2sxc ca. 6 or 7
        var inputType = definition.Metadata.GetBestValue<string>(GeneralFieldInputType, TypeGeneral);
        if (inputType.HasValue()) return inputType;
            
        // if not available, check older metadata, where it was on the @String
        inputType = definition.Metadata.GetBestValue<string>(GeneralFieldInputType, TypeString);
        // if found, check and maybe add prefix string
        const string prefix = "string-";
        if (inputType.HasValue() && !inputType.StartsWith(prefix))
            return $"{prefix}{inputType}";

        // if still not found, assemble from known type
        if (inputType.HasValue()) return inputType;
            
        return definition.Type.ToString().ToLowerInvariant() + "-default";
    }

    public static string EntityFieldItemTypePrimary(this IContentTypeAttribute attribute)
    {
        var itemTypeName = attribute.Metadata.GetBestValue<string>(AttributeNames.EntityFieldType) ?? "";
        var typeName = itemTypeName.Split(',').First().Trim();
        return typeName;
    }

    /// <summary>
    /// Check if an attribute has formulas.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static bool HasFormulas(this IContentTypeAttribute attribute, ILog log)
    {
        var l = log.Fn<bool>(attribute.Name);
        var allMd = attribute.Metadata.FirstOrDefaultOfType(AttributeMetadataConstants.TypeGeneral);
        if (allMd == null) return l.ReturnFalse("no @All");

        var calculationsAttr = allMd.Attributes.Values.FirstOrDefault(a => a.Name == AttributeMetadataConstants.MetadataFieldAllFormulas);
        if (calculationsAttr == null) return l.ReturnFalse("no calc property");

        var calculations = calculationsAttr.Values?.FirstOrDefault()?.ObjectContents as IEnumerable<IEntity>;
        return l.Return(calculations?.Any() ?? false);
    }

}