using System;
using System.Collections.Generic;
using System.Globalization;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Value
    /// </summary>
    public class Value
    {
        public IList<ILanguage> Languages { get; private set; }

        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        public static IValue Build(string attributeType, object value, List<ILanguage> languages, IDeferredEntitiesList fullEntityListForLookup = null)
        {
            if (languages == null) languages = new List<ILanguage>();
            Value typedModel;
            var stringValue = value as string;
            try
            {
                var type = (AttributeTypeEnum)Enum.Parse(typeof(AttributeTypeEnum), attributeType);
                switch (type)
                {
                    case AttributeTypeEnum.Boolean: 
                        typedModel = new Value<bool?>(string.IsNullOrEmpty(stringValue) ? (bool?)null : bool.Parse(stringValue));
                        break;
                    case AttributeTypeEnum.DateTime: 
                        typedModel = new Value<DateTime?>(string.IsNullOrEmpty(stringValue) ? (DateTime?)null : DateTime.Parse(stringValue));
                        break;
                    case AttributeTypeEnum.Number:
                        typedModel = new Value<decimal?>(string.IsNullOrEmpty(stringValue) ? (decimal?)null : decimal.Parse(stringValue, CultureInfo.InvariantCulture));
                        break;
                    case AttributeTypeEnum.Entity: 
                        var entityIds = value as IEnumerable<int?>;
                        typedModel = new Value<EntityRelationship>(new EntityRelationship(fullEntityListForLookup, entityIds));
                        break;
                    // ReSharper disable RedundantCaseLabel
                    case AttributeTypeEnum.String:  // most common case
                    case AttributeTypeEnum.Empty:   // empty - should actually not contain anything!
                    case AttributeTypeEnum.Custom:  // custom value, currently just parsed as string for manual processing as needed
                    case AttributeTypeEnum.Hyperlink:// special case, handled as string
                    // ReSharper restore RedundantCaseLabel
                    default:
                        typedModel = new Value<string>(stringValue);
                        break;
                }
            }
            catch
            {
                return new Value<string>(stringValue);
            }

            typedModel.Languages = languages;

            return (IValue)typedModel;
        }


        internal static readonly Value<EntityRelationship> NullRelationship = new Value<EntityRelationship>(new EntityRelationship(null))
        {
            Languages = new List<ILanguage>()
        };

    }
}