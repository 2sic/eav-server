namespace ToSic.Eav.Data.Internal;

[PrivateApi("WIP")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContentTypeAttributeSpecsAttribute: Attribute
{
    /// <summary>
    /// Attribute name. Usually not set, as it's the name of the property
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type of the value.
    /// Only set this if it must be different from the property type
    /// or if the property type has an unexpected type.
    /// Also use this for relationship (Entity) fields.
    /// </summary>
    public ValueTypes Type { get; set; }

    /// <summary>
    /// If the attribute is for the title.
    /// </summary>
    public bool IsTitle { get; set; }

    /// <summary>
    /// Optional description.
    /// </summary>
    public string Description { get; set; }

    //public int SortOrder { get; set; }
}