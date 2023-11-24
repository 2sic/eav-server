namespace ToSic.Eav.SysData;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Requirement
{
    public Requirement(string type, string nameId)
    {
        Type = type;
        NameId = nameId;
    }

    public string Type { get; set; }

    /// <summary>
    /// The string identifier of this condition
    /// </summary>
    public string NameId { get; set; }
}