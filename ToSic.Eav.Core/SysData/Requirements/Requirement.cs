namespace ToSic.Eav.SysData;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Requirement(string type, string nameId)
{
    public string Type { get; set; } = type;

    /// <summary>
    /// The string identifier of this condition
    /// </summary>
    public string NameId { get; set; } = nameId;
}