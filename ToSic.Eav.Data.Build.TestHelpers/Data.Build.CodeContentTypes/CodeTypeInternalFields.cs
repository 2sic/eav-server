namespace ToSic.Eav.Data.Build.CodeContentTypes;

/// <summary>
/// Properties such as ID and Guid should not be used for attributes.
/// </summary>
public class CodeTypeInternalFields
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    /// <summary>
    /// In this test, this is the only field that should result in an Attribute.
    /// </summary>
    public string Name { get; set; }

    public DateTime Created { get; set; }

    public DateTime Modified { get; set; }

}