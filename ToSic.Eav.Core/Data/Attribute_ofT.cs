using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute / Property of an Entity with Values of a Generic Type
    /// </summary>
    /// <typeparam name="TType">Type of the Value</typeparam>
    public class Attribute<TType> : AttributeBase, IAttribute<TType>
    {
        public Attribute(string name, string type) : base(name, type) { }

        /// <inheritdoc/>
        public IList<IValue> Values { get; set; } = new List<IValue>();

        /// <inheritdoc/>
        public TType TypedContents
        {
            get
            {
				// Prevent Exception if Values is null
	            if (Values == null)
		            return default;

                try
                {
                    var value = (IValue<TType>)Values.FirstOrDefault();
                    return value != null ? value.TypedContents : default;
                }
                catch
                {
                    return default;
                }
            }
        }

        /// <inheritdoc/>
        public IList<IValue<TType>> Typed => Values.Cast<IValue<TType>>().ToList();

        /// <inheritdoc/>
        public TType this[int languageId] => this[new[] { languageId }];

        #region IAttribute Implementations
        [PrivateApi]
        object IAttribute.this[int[] languageIds] => this[languageIds];

        [PrivateApi]
        object IAttribute.this[string languageKey] => this[languageKey];
        [PrivateApi]
        object IAttribute.this[string[] languageKeys] => this[languageKeys];
        [PrivateApi]
        object IAttribute.this[int languageId] => this[languageId];
        #endregion

        /// <inheritdoc/>
        public TType this[int[] languageIds]
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
                return TypedContents == null ? default : TypedContents;
            }
        }

        /// <inheritdoc/>
        public TType this[string languageKey] => this[new[] { languageKey }];

        /// <inheritdoc/>
        public TType this[string[] languageKeys]
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
                        } // may occur for nullable types
                }
                // use Default
                return TypedContents == null ? default(TType) : TypedContents;
            }
        }

        [PrivateApi]
        public IAttribute Copy() => new Attribute<TType>(Name, Type) { Values = Values.Select(v => v.Copy(Type)).ToList()};
    }
}