using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute with Values of a Generic Type
    /// </summary>
    /// <typeparam name="ValueType">Type of the Value</typeparam>
    public class Attribute<ValueType> : AttributeBase, IAttribute<ValueType>, IAttributeManagement
    {
        public Attribute(string name, string type, bool isTitle, int attributeId, int sortOrder) : base(name, type, isTitle, attributeId, sortOrder) { }

        public IEnumerable<IValue> Values { get; set; }
        public IValueManagement DefaultValue { get; set; }

        public ValueType TypedContents
        {
            get
            {
				// Prevent Exception if Values is null
	            if (Values == null)
		            return default(ValueType);

                try
                {
                    var value = (IValue<ValueType>)Values.FirstOrDefault();
                    return value != null ? value.TypedContents : default(ValueType);
                }
                catch
                {
                    return default(ValueType);
                }
            }
        }

        public ITypedValue<ValueType> Typed
        {
            get { return new TypedValue<ValueType>(Values, TypedContents); }
        }

        public object this[int languageId]
        {
            get { return this[new[] { languageId }]; }
        }

        public object this[int[] languageIds]
        {
            get
            {
                // Value with Dimensions specified
                if (languageIds != null && languageIds.Length > 0 && Values != null)
                {
                    // try match all specified Dimensions
                    var valueHavingSpecifiedLanguages = Values.FirstOrDefault(va => languageIds.All(di => va.Languages.Select(d => d.DimensionId).Contains(di)));
                    if (valueHavingSpecifiedLanguages != null)
                    {
                        try
                        {
                            return ((IValue<ValueType>)valueHavingSpecifiedLanguages).TypedContents;
                        }
                        catch (InvalidCastException) { } // may occour for nullable types
                    }
                }
                // use Default
                return TypedContents == null ? default(ValueType) : TypedContents;
            }
        }

        public object this[string languageKey]
        {
            get { return this[new[] { languageKey }]; }
        }

        public object this[string[] languageKeys]
        {
            get
            {
                // Value with Dimensions specified
                if (languageKeys != null && languageKeys.Length > 0 && Values != null)
                {
                    // try match all specified Dimensions
                    var valueHavingSpecifiedLanguages = Values.FirstOrDefault(va => languageKeys.All(vk => va.Languages.Select(d => d.Key).Contains(vk.ToLower())));
                    if (valueHavingSpecifiedLanguages != null)
                    {
                        try
                        {
                            return ((IValue<ValueType>)valueHavingSpecifiedLanguages).TypedContents;
                        }
                        catch (InvalidCastException) { }	// may occour for nullable types
                    }
                }
                // use Default
                return TypedContents == null ? default(ValueType) : TypedContents;
            }
        }
    }
}