namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavAttributesInSets
{
    public int AttributeId { get; set; }

    public int AttributeSetId { get; set; }

    public int SortOrder { get; set; }

    public bool IsTitle { get; set; } = false;

    public virtual ToSicEavAttributes Attribute { get; set; }

    public virtual ToSicEavAttributeSets AttributeSet { get; set; }
}