namespace ToSic.Eav.Data;

/// <summary>
/// Represents properties we should know about Attributes. This is the base for both
/// - <see cref="IContentTypeAttribute"/> (in the IContentType)
/// - attribute with values-list (in the IEntity)
/// </summary>
/// <remarks>
/// > We recommend you read about the [](xref:Basics.Data.Index)
/// </remarks>
[PublicApi]
public interface IAttributeBase
{
    /// <summary>
    /// Name of the Attribute
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The official type, as a controlled (enum) value.
    /// </summary>
    ValueTypes Type { get; }
}