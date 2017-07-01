using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav
{/// <summary>
	/// Represents an Attribute of a Generic Type
	/// </summary>
	/// <typeparam name="T">Type of the Value</typeparam>
	public interface IAttribute<T> : IAttribute
	{
		/// <summary>
		/// Gets the typed first/default value
		/// </summary>
		T TypedContents { get; }
		/// <summary>
		/// Gets the typed Value
		/// </summary>
		IList<IValue<T>> Typed { get; }
	}
}
