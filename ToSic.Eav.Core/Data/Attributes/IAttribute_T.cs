using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
	/// Represents an Attribute (Property), but strongly typed
	/// </summary>
    /// <remarks>
    /// > We recommend you read about the [](xref:Basics.Data.Index)
    /// </remarks>
	/// <typeparam name="T">Type of the Value</typeparam>
	[PublicApi_Stable_ForUseInYourCode]
	public interface IAttribute<out T> : IAttribute
	{
		/// <summary>
		/// Gets the typed first/default value
		/// </summary>
		T TypedContents { get; }

        /// <summary>
        /// Gets the typed Value Objects - so the same as Values, but with the correct type
        /// </summary>
        IEnumerable<IValue<T>> Typed { get; }

        /// <summary>
        /// Gets the Value for the specified Language/Dimension using the ID accessor. Usually not needed. Typed.
        /// </summary>
        /// <param name="languageId">the language id (number)</param>
        new T this[int languageId] { get; }

        #region 2dm Removed Accessors which I believe were only internal and never used!

        ///// <summary>
        ///// Gets the Value for this Languages, typed
        ///// </summary>
        ///// <param name="languageIds">list of languages to check</param>
        //T this[int[] languageIds] { get; }

        ///// <summary>
        ///// Get the best/first matching value for the specified language key - typed
        ///// </summary>
        ///// <param name="languageKey">The language key (string) to look for</param>
        //new T this[string languageKey] { get; }

        ///// <summary>
        ///// Get the best/first matching value for the specified language keys - typed
        ///// </summary>
        ///// <param name="languageKeys">list of language keys</param>
        //new T this[string[] languageKeys] { get; }

        #endregion

    }
}
