using ToSic.Eav.Data.Sys.Entities;
using static ToSic.Eav.Data.Sys.Attributes.AttributeMetadataConstants;

namespace ToSic.Eav.Data.Sys.ContentTypes;

[PrivateApi]
// ReSharper disable once InconsistentNaming
public static class IContentTypeAttributeExtensions
{
    /// <param name="definition"></param>
    extension(IContentTypeAttribute definition)
    {
        /// <summary>
        /// Will look up the input type name of an attribute. It uses a series of cascades, because historically this can be missing or written in a different way.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It's important to NOT cache this result, because it can change during runtime, and then a cached info would be wrong. 
        /// </remarks>
        internal string GetInputType()
        {
            // Preferred storage and available in all fields defined after 2sxc ca. 6 or 7
            var inputType = definition.Metadata.Get<string>(GeneralFieldInputType, typeName: TypeGeneral);
            if (inputType.HasValue())
                return inputType;
            
            // if not available, check older metadata, where it was on the @String
            inputType = definition.Metadata.Get<string>(GeneralFieldInputType, typeName: TypeString);
            // if found, check and maybe add prefix string
            const string prefix = "string-";
            if (inputType.HasValue() && !inputType.StartsWith(prefix))
                return $"{prefix}{inputType}";

            // if still not found, assemble from known type
            if (inputType.HasValue())
                return inputType;
            
            return definition.Type.ToString().ToLowerInvariant() + "-default";
        }

        public string EntityFieldItemTypePrimary()
        {
            var itemTypeName = definition.Metadata.Get<string>(AttributeNames.EntityFieldType) ?? "";
            var typeName = itemTypeName.Split(',').First().Trim();
            return typeName;
        }

        /// <summary>
        /// Check if an attribute has formulas.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool HasFormulas(ILog log)
        {
            var l = log.Fn<bool>(definition.Name);
            var allMd = definition.Metadata.GetOne(TypeGeneral);
            if (allMd == null)
                return l.ReturnFalse("no @All");

            var calculationsAttr = allMd.Attributes.Values.FirstOrDefault(a => a.Name == MetadataFieldAllFormulas);
            if (calculationsAttr == null)
                return l.ReturnFalse("no calc property");

            var calculations = calculationsAttr.Values.FirstOrDefault()?.ObjectContents as IEnumerable<IEntity>;
            return l.Return(calculations?.Any() ?? false);
        }
    }
}