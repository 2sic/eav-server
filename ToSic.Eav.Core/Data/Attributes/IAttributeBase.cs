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

        // Removed 2023-03-02 2dm - keep comment in till 2023q3 because it is technically a breaking change
        ///// <summary>
        ///// Type of the Attribute like 'string', 'decimal' etc.
        ///// </summary>
        //string Type { get; }

        /// <summary>
        /// The official type, as a controlled (enum) value.
        /// </summary>
        /// <remarks>
        /// This property `Type` used to be a string containing the same thing as now `Type.ToString()` does.
        /// It was changed (breaking change) in v15
        /// </remarks>
        ValueTypes Type { get; }

    }
}
