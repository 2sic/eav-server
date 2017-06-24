namespace ToSic.Eav.Enums
{
	/// <summary>
	/// Attribute Type Enum
	/// </summary>
	public enum AttributeTypeEnum
	{
        /// <summary> Used for unknown cases, where you would otherwise use null </summary>
        Undefined,  // note: must be first! this is important, otherwise certain code will break as the first value is the "default" / null-value


		/// <summary>Boolean Attribute Type</summary>
		Boolean,
		/// <summary>DateTime Attribute Type</summary>
		DateTime,
		/// <summary>Entity Attribute Type</summary>
		Entity,
		/// <summary>Hyperlink Attribute Type</summary>
		Hyperlink,
		/// <summary>Number Attribute Type</summary>
		Number,
		/// <summary>String Attribute Type</summary>
		String,

        /// <summary>Empty for titles etc. </summary>
        Empty,

        /// <summary> Custom data-type - for custom serialization, will store string </summary>
        Custom

	}
}
