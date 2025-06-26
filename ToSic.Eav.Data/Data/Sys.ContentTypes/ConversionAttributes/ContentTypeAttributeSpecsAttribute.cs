namespace ToSic.Eav.Data.Sys.ContentTypes;

[PrivateApi("WIP")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ContentTypeAttributeSpecsAttribute: Attribute
{
    /// <summary>
    /// Attribute name. Usually not set, as it's the name of the property
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The type of the value.
    /// Only set this if it must be different from the property type
    /// or if the property type has an unexpected type.
    /// Also use this for relationship (Entity) fields.
    /// </summary>
    public ValueTypes Type { get; init; }

    /// <summary>
    /// If the attribute is for the title.
    /// </summary>
    public bool IsTitle { get; init; }

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; init; }

    //public int SortOrder { get; set; }

    public string? InputTypeWIP { get; init; }
}