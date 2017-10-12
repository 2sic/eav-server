using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an Attribute with Values
    /// </summary>
    public interface IAttribute : IAttributeBase
	{
		/// <summary>
		/// Gets a IEnumerable of all Values of this Entity's Attribute
		/// </summary>
		IList<IValue> Values { get; set; } // temp set, must find out how to remove this again form the interface

        #region 2017-06-11 2dm turned off / moved to typed...
        /// <summary>
        /// Gets the Value for this Language
        /// </summary>
        object this[int languageId] { get; }
        /// <summary>
        /// Gets the Value for this Languages
        /// </summary>
        object this[int[] languageIds] { get; }
        /// <summary>
        /// Gets the Value for this Language
        /// </summary>
        object this[string languageKey] { get; }
        /// <summary>
        /// Gets the Value for this Languages
        /// </summary>
        object this[string[] languageKeys] { get; }
        #endregion

	    IAttribute Copy();
	}
}
