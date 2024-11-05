namespace ToSic.Eav.Data.Internal;

[PrivateApi("WIP")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class)]
public class ContentTypeSpecsAttribute: Attribute
{
    /// <summary>
    /// Content Type Guid, as a string, because GUIDs are not supported in attributes
    /// </summary>
    public string Guid { get; set; }

    /// <summary>
    /// Content type name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Content type description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Content Type Scope - if blank, will default to "Default"
    /// </summary>
    public string Scope { get; set; }
}