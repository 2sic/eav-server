﻿using System.Linq;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using static ToSic.Eav.Data.AttributeMetadata;

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
            var itemTypeName = attribute.Metadata.GetBestValue<string>(Attributes.EntityFieldType) ?? "";
            var typeName = itemTypeName.Split(',').First().Trim();
            return typeName;
        }

    }
}
