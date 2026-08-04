namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// Content type information for use when converting a POCO to a Content Type.
/// </summary>
[WorkInProgressApi("v22")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class ContentTypeAttribute : Attribute
{
    /// <summary>
    /// Content Type Guid **required**.
    /// Enter as a string, because GUIDs are not supported in C# attributes.
    /// </summary>
    public required string Guid { get; set; }

    /// <summary>
    /// Content type name **required**.
    /// If not set, will use the class name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Content type description.
    /// It's highly recommended for the future you who must diagnose something.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Content Type Scope - if blank, will default to "Default"
    /// </summary>
    public string Scope { get; set; } = "";
}