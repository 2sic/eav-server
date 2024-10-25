namespace ToSic.Eav.Data.ContentTypes.CodeAttributes;

[PrivateApi("WIP")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContentTypeAttributeAttribute: Attribute
{
    public string Name { get; set; }
    public ValueTypes Type { get; set; }
    public bool IsTitle { get; set; }
    public int SortOrder { get; set; }
    public string Description { get; set; }
}