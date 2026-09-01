namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// Content type specifications when converting the schema of an object to a Content Type Definition.
/// </summary>
/// <remarks>
/// This is mainly important to assign it a distinct name and GUID,
/// mainly for marking the generated Entities to allow future auto-conversion to Models.
/// </remarks>
[WorkInProgressApi("v22")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class ContentTypeAttribute : Attribute
{
    /// <summary>
    /// Content Type Guid **required**.
    /// </summary>
    /// <remarks>
    /// Enter as a string, because GUIDs are not supported in C# attributes.
    ///
    /// It will later be on the property `ContentType.NameId`.
    /// </remarks>
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