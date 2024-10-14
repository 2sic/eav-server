namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavAttributesInSets
{
    public ToSicEavAttributesInSets()
    {
        IsTitle = false;
    }

    public int AttributeId { get; set; }
    public int AttributeSetId { get; set; }
    //public int AttributeGroupId { get; set; }
    public int SortOrder { get; set; }
    public bool IsTitle { get; set; }

    //public virtual ToSicEavAttributeGroups AttributeGroup { get; set; }
    public virtual ToSicEavAttributes Attribute { get; set; }
    public virtual ToSicEavAttributeSets AttributeSet { get; set; }
}