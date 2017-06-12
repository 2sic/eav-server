using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute / Property of an Entity with Values of a Generic Type
    /// </summary>
    /// <typeparam name="TType">Type of the Value</typeparam>
    public class Attribute<TType> : AttributeBase, IAttribute<TType>
    {
        public Attribute(string name, string type/*, bool isTitle*/) : base(name, type/*, isTitle*/) { }

        public IEnumerable<IValue> Values { get; set; }

        public TType TypedContents
        {
            get
            {
				// Prevent Exception if Values is null
	            if (Values == null)
		            return default(TType);

                try
                {
                    var value = (IValue<TType>)Values.FirstOrDefault();
                    return value != null ? value.TypedContents : default(TType);
                }
                catch
                {
                    return default(TType);
                }
            }
        }

        public IValueOfDimension<TType> Typed => new TypedValue<TType>(Values, TypedContents);

        public object this[int languageId] => this[new[] { languageId }];

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
                        try
                        {
                            return ((IValue<TType>) valueHavingSpecifiedLanguages).TypedContents;
                        }
                        catch (InvalidCastException)
                        {
                        } // may occour for nullable types
                }
                // use Default
                return TypedContents == null ? default(TType) : TypedContents;
            }
        }

        public object this[string languageKey] => this[new[] { languageKey }];

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
                        try
                        {
                            return ((IValue<TType>) valueHavingSpecifiedLanguages).TypedContents;
                        }
                        catch (InvalidCastException)
                        {
                        } // may occour for nullable types
                }
                // use Default
                return TypedContents == null ? default(TType) : TypedContents;
            }
        }
    }
}