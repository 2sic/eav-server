using System.Collections.Generic;

namespace ToSic.Eav
{
	/// <summary>
	/// Represents an Attribute
	/// </summary>
	public interface IAttributeBase
	{
		/// <summary>
		/// Name of the Attribute
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Type of the Attribute
		/// </summary>
		string Type { get; }
        AttributeTypeEnum ControlledType { get; }

        bool IsTitle { get; }

        // additional info for the persistence layer
        int AttributeId { get; }

        int SortOrder { get; }

    }

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

	/// <summary>
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
		ITypedValue<T> Typed { get; }
	}

	/// <summary>
	/// Represents a typed Value
	/// </summary>
	/// <typeparam name="T">Type of the Value</typeparam>
	public interface ITypedValue<T>
	{
		/// <summary>
		/// Gets the Value for this Language
		/// </summary>
		T this[int languageId] { get; }
		/// <summary>
		/// Gets the Value for this Languages
		/// </summary>
		T this[int[] languageIds] { get; }
		/// <summary>
		/// Gets the Value for this Language
		/// </summary>
		T this[string languageKey] { get; }
		/// <summary>
		/// Gets the Value for this Languages
		/// </summary>
		T this[string[] languageKeys] { get; }
	}

	/// <summary>
	/// Represents an Attribute for management purposes
	/// </summary>
	public interface IAttributeManagement : IAttribute
	{
		/// <summary>
		/// Gets or sets whether the Attribute is the Title Attribute in the Attribute Set
		/// </summary>
		bool IsTitle { get; set; }
		/// <summary>
		/// Sets the Name of the Attribute
		/// </summary>
		new string Name { set; }
		/// <summary>
		/// Sets the Type of the Attribute
		/// </summary>
		new string Type { set; }
		/// <summary>
		/// Sets the Values of this Attribute
		/// </summary>
		new IEnumerable<IValue> Values { set; }
		/// <summary>
		/// Gets or sets the Default Value
		/// </summary>
		IValueManagement DefaultValue { get; set; }
	}
}
