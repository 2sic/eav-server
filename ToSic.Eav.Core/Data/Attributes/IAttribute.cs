using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute with Values - without knowing what data type is in the value.
    /// Usually we'll extend this and use <see cref="IAttribute{T}"/> instead.
    /// </summary>
    /// <remarks>
    /// > We recommend you read about the @Specs.Data.Intro
    /// </remarks>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IAttribute : IAttributeBase
	{
		/// <summary>
		/// Gets a list of all <see cref="IValue"/>s of this Entity's Attribute. To get the typed objects, use the <see cref="IAttribute{T}.Typed"/>
		/// </summary>
		IList<IValue> Values { get; set; } 

        #region get-value eaccessors

        /// <summary>
        /// Gets the Value for the specified Language/Dimension using the ID accessor. Usually not needed. Untyped.
        /// </summary>
        /// <param name="languageId">the language id (number)</param>
        [PrivateApi]
        object this[int languageId] { get; }

        ///// <summary>
        ///// Gets the Value for this Languages, untyped
        ///// </summary>
        ///// <param name="languageIds">list of languages to check</param>
        //[PrivateApi]
        //object this[int[] languageIds] { get; }

        /// <summary>
        /// Get the best/first matching value for the specified language key - untyped
        /// </summary>
        /// <param name="languageKey">The language key (string) to look for</param>
        [PrivateApi]
        object this[string languageKey] { get; }

        /// <summary>
        /// Get the best/first matching value for the specified language keys - untyped
        /// </summary>
        /// <param name="languageKeys">list of language keys</param>
        [PrivateApi]
        object this[string[] languageKeys] { get; }

        #endregion

        /// <summary>
        /// Clone the attribute - for creating copies of this, usually when saving drafts or similar.
        /// </summary>
        /// <returns></returns>
        [PrivateApi("might rename to Clone or something, and might be moved out")]
	    IAttribute Copy();
	}
}
