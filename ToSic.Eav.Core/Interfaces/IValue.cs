using System.Collections.Generic;

namespace ToSic.Eav
{
    /// <summary>
    /// Represents a Value
    /// </summary>
    public interface IValue
    {
        /// <summary>
        /// Gets the Languages assigned to this Value
        /// </summary>
        IEnumerable<ILanguage> Languages { get; }
        string Serialized {get;}

        object SerializableObject { get; }
	}

	/// <summary>
	/// Represents a Value
	/// </summary>
	/// <typeparam name="T">Type of the actual Value</typeparam>
	public interface IValue<T> : IValue
	{
		/// <summary>
		/// Typed contents
		/// </summary>
		T TypedContents { get; }
	}

	/// <summary>
	/// Represents a Value for Management purposes
	/// </summary>
	public interface IValueManagement
	{
		/// <summary>
		/// Sets the Languages assigned to this Value
		/// </summary>
		IEnumerable<ILanguage> Languages { set; }
		/// <summary>
		/// Gets or sets the internal ChangeLogId when this value was created. Used to find out which was the first one created.
		/// </summary>
		int ChangeLogIdCreated { get; set; }
		/// <summary>
		/// Gets or sets the internal ValueId
		/// </summary>
		int ValueId { get; set; }
	}
}
