namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavAttributeTypes
{
    public ToSicEavAttributeTypes()
    {
        ToSicEavAttributes = new HashSet<ToSicEavAttributes>();
    }

    public string Type { get; set; }

    public virtual ICollection<ToSicEavAttributes> ToSicEavAttributes { get; set; }
}