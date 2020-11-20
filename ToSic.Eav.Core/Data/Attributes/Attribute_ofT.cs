using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute / Property of an Entity with Values of a Generic Type
    /// </summary>
    /// <remarks>
    /// > We recommend you read about the @Specs.Data.Intro
    /// </remarks>
    /// <typeparam name="T">Type of the Value</typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi, use interface IAttribute<T>")]
    public class Attribute<T> : AttributeBase, IAttribute<T>
    {
        /// <summary>
        /// Create an attribute object - usually when building up the data-model for caching.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public Attribute(string name, string type) : base(name, type) { }

        /// <inheritdoc/>
        public IList<IValue> Values { get; set; } = new List<IValue>();

        /// <inheritdoc/>
        public T TypedContents
        {
            get
            {
				// Prevent Exception if Values is null
	            if (Values == null)
		            return default;

                try
                {
                    var value = (IValue<T>)Values.FirstOrDefault();
                    return value != null ? value.TypedContents : default;
                }
                catch
                {
                    return default;
                }
            }
        }

        /// <inheritdoc/>
        public IList<IValue<T>> Typed => Values.Cast<IValue<T>>().ToList();

        /// <inheritdoc/>
        public T this[int languageId] => this[new[] { languageId }];

        #region IAttribute Implementations
        [PrivateApi]
        //object IAttribute.this[int[] languageIds] => this[languageIds];

        [PrivateApi]
        object IAttribute.this[string languageKey] => this[languageKey];
        [PrivateApi]
        object IAttribute.this[string[] languageKeys] => this[languageKeys];
        [PrivateApi]
        object IAttribute.this[int languageId] => this[languageId];
        #endregion

        /// <inheritdoc/>
        public T this[int[] languageIds]
        {
            get
            {
                // Value with Dimensions specified
                if (languageIds != null && languageIds.Length > 0 && Values != null)
                {
                    // try match all specified Dimensions
                    // note that as of now, the dimensions are always just 1 language, not more
                    // so the dimensions are _not_ a list of languages, but would contain other dimensions
                    // that is why we match ALL - but in truth it's a "feature" that's never been used
                    var valueHavingSpecifiedLanguages = Values.FirstOrDefault(va => languageIds.All(di => va.Languages.Select(d => d.DimensionId).Contains(di)));
                    if (valueHavingSpecifiedLanguages != null)
                        try
                        {
                            return ((IValue<T>) valueHavingSpecifiedLanguages).TypedContents;
                        }
                        catch (InvalidCastException) { /* ignore, may occur for nullable types */ }
                }
                // use Default
                return TypedContents == null ? default : TypedContents;
            }
        }

        /// <inheritdoc/>
        public T this[string languageKey] => this[new[] { languageKey }];

        /// <inheritdoc/>
        public T this[string[] languageKeys]
        {
            get
            {
                // Value with Dimensions specified
                if (languageKeys != null && languageKeys.Length > 0 && Values != null)
                {
                    // ensure language Keys in lookup-list are lowered
                    var langsLower = languageKeys.Select(l => l.ToLower()).ToArray();
                    // try match all specified Dimensions
                    // note that as of now, the dimensions are always just 1 language, not more
                    // so the dimensions are _not_ a list of languages, but would contain other dimensions
                    // that is why we match ALL - but in truth it's a "feature" that's never been used
                    var valueHavingSpecifiedLanguages = Values
                        .FirstOrDefault(va => langsLower.All(lng => va.Languages.Select(d => d.Key).Contains(lng)));
                    if (valueHavingSpecifiedLanguages != null)
                        try
                        {
                            return ((IValue<T>) valueHavingSpecifiedLanguages).TypedContents;
                        }
                        catch (InvalidCastException) { /* ignore, may occur for nullable types */ } 
                }
                // use Default
                return TypedContents == null ? default(T) : TypedContents;
            }
        }

        [PrivateApi]
        public IAttribute Copy() => new Attribute<T>(Name, Type) { Values = Values.Select(v => v.Copy(Type)).ToList()};
    }
}