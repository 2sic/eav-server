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
        /// The official type, as a controlled (enum) value.
        /// </summary>
        /// <remarks>
        /// This property `Type` used to be a string containing the same thing as now `Type.ToString()` does.
        /// It was changed in v15 (breaking change)
        /// </remarks>
        ValueTypes Type { get; }

    }
}
