using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav
{
    /// <summary>
    /// Represents an Attribute with Values
    /// </summary>
    public interface IAttribute : IAttributeBase
	{
		/// <summary>
		/// Gets a IEnumerable of all Values of this Entity's Attribute
		/// </summary>
		IEnumerable<IValue> Values { get; }
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
	}
}
