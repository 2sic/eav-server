using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents properties we should know about Attributes. This is the base for both
    /// - <see cref="IContentTypeAttribute"/> (in the IContentType)
    /// - attribute with values-list (in the IEntity)
	/// </summary>
	/// <remarks>
    /// > We recommend you read about the [](xref:Basics.Data.Index)
	/// </remarks>
    [PublicApi_Stable_ForUseInYourCode]
	public interface IAttributeBase
	{
		/// <summary>
		/// Name of the Attribute
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Type of the Attribute like 'string', 'decimal' etc.
		/// </summary>
		string Type { get; }

        /// <summary>
        /// The official type, as a controlled value
        /// </summary>
        ValueTypes ControlledType { get; }

    }
}
