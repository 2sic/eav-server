using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav
{
	/// <summary>
	/// Represents an Attribute for management purposes
	/// </summary>
	public interface IAttributeManagement : IAttribute
	{
		///// <summary>
		///// Gets or sets whether the Attribute is the Title Attribute in the Attribute Set
		///// </summary>
		//bool IsTitle { get; set; }

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
		IValue DefaultValue { get; set; }
	}
}
