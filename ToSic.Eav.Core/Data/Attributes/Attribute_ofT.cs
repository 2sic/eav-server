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
	            if (Values == null) return default;

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
        public T this[int languageId] => GetInternal(new[] { languageId }, FindHavingDimensions);

        #region IAttribute Implementations
        [PrivateApi]
        object IAttribute.this[string languageKey] => GetInternal(new [] {languageKey}, FindHavingDimensions);

        [PrivateApi]
        object IAttribute.this[string[] languageKeys] => GetInternal(languageKeys, FindHavingDimensions);
        [PrivateApi]
        object IAttribute.this[int languageId] => this[languageId];
        #endregion

        /// <inheritdoc/>
        public T this[int[] languageIds] => GetInternal(languageIds, FindHavingDimensions);


        /// <inheritdoc/>
        public T this[string languageKey] => GetInternal(new[] { languageKey }, FindHavingDimensions);

        /// <inheritdoc/>
        public T this[string[] languageKeys] => GetInternal(languageKeys, FindHavingDimensions);

        private T GetInternal<TKey>(TKey[] keys, Func<TKey[], IValue> lookupCallback)
        {
            // Value with Dimensions specified
            if (keys != null && keys.Length > 0 && Values != null && Values.Count > 0)
            {
                // try match all specified Dimensions
                // note that as of now, the dimensions are always just 1 language, not more
                // so the dimensions are _not_ a list of languages, but would contain other dimensions
                // that is why we match ALL - but in truth it's a "feature" that's never been used
                IValue valueHavingSpecifiedLanguages = null;
                foreach (var key in keys)
                {
                    // if it's null or 0, try to just get anything
                    if (EqualityComparer<TKey>.Default.Equals(key, default))
                        valueHavingSpecifiedLanguages = Values.FirstOrDefault();
                    else if (key != null)
                        valueHavingSpecifiedLanguages = lookupCallback(new[] {key});
                    
                    // stop at first hit
                    if (valueHavingSpecifiedLanguages != null) break;
                }

                if (valueHavingSpecifiedLanguages != null)
                    try
                    {
                        return ((IValue<T>)valueHavingSpecifiedLanguages).TypedContents;
                    }
                    catch (InvalidCastException) { /* ignore, may occur for nullable types */ }
            }
            // use Default
            return TypedContents == null ? default : TypedContents;

        }

        private IValue FindHavingDimensions(int[] keys)
        {
            var valuesHavingDimensions = Values.FirstOrDefault(va => keys.All(di => va.Languages.Select(d => d.DimensionId).Contains(di)));
            return valuesHavingDimensions;
        }

        private IValue FindHavingDimensions(string[] keys)
        {
            // ensure language Keys in lookup-list are lowered
            var langsLower = keys.Select(l => l.ToLowerInvariant()).ToArray();
            var valuesHavingDimensions = Values
                .FirstOrDefault(va => langsLower.All(lng => va.Languages.Select(d => d.Key).Contains(lng)));
            return valuesHavingDimensions;
        }

        [PrivateApi]
        public IAttribute Copy() => new Attribute<T>(Name, Type) { Values = Values.Select(v => v.Copy(Type)).ToList()};
    }
}