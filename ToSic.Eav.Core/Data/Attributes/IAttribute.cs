using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute with Values - without knowing what data type is in the value.
    /// Usually we'll extend this and use <see cref="IAttribute{T}"/> instead.
    /// </summary>
    /// <remarks>
    /// > We recommend you read about the [](xref:Basics.Data.Index)
    /// </remarks>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IAttribute : IAttributeBase
	{
		/// <summary>
		/// Gets a list of all <see cref="IValue"/>s of this Entity's Attribute. To get the typed objects, use the <see cref="IAttribute{T}.Typed"/>
		/// </summary>
		IEnumerable<IValue> Values { get; } 

        #region get-value eaccessors

        /// <summary>
        /// Gets the Value for the specified Language/Dimension using the ID accessor. Usually not needed. Untyped.
        /// </summary>
        /// <param name="languageId">the language id (number)</param>
        [PrivateApi]
        object this[int languageId] { get; }

        /// <summary>
        /// Get the best/first matching value for the specified language key - untyped
        /// </summary>
        /// <param name="languageKey">The language key (string) to look for</param>
        [PrivateApi]
        object this[string languageKey] { get; }


        #endregion

        #region 2dm Removed Accessors which I believe were only internal and never used!

        ///// <summary>
        ///// Get the best/first matching value for the specified language keys - untyped
        ///// </summary>
        ///// <param name="languageKeys">list of language keys</param>
        //[PrivateApi]
        //object this[string[] languageKeys] { get; }

        #endregion


        [PrivateApi("experimental in 12.05")]
        (IValue ValueField, object Result) GetTypedValue(string[] languageKeys);

        [PrivateApi("internal only")]
        IAttribute CloneWithNewValues(IImmutableList<IValue> values);
    }
}
