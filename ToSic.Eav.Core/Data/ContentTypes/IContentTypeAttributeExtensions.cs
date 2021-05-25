using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
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
            var inputType = definition.Metadata.GetBestValue<string>(
                AttributeMetadata.GeneralFieldInputType, AttributeMetadata.TypeGeneral);

            // if not available, check older metadata, where it was on the @String
            if (string.IsNullOrWhiteSpace(inputType))
            {
                inputType = definition.Metadata.GetBestValue<string>(
                    AttributeMetadata.GeneralFieldInputType, AttributeMetadata.TypeString);
                // if found, check and maybe add prefix string
                const string prefix = "string-";
                if (!string.IsNullOrWhiteSpace(inputType) && !inputType.StartsWith(prefix))
                    inputType = prefix + inputType;
            }

            // if still not found, assemble from known type
            if (string.IsNullOrWhiteSpace(inputType))
                inputType = definition.Type.ToLowerInvariant() + "-default";

            return inputType;
        }

        public static string EntityFieldItemTypePrimary(this IContentTypeAttribute attribute)
        {
            var itemTypeName = attribute.Metadata.GetBestValue<string>(Attributes.EntityFieldType) ?? "";
            var typeName = itemTypeName.Split(',').First().Trim();
            return typeName;
        }

    }
}
