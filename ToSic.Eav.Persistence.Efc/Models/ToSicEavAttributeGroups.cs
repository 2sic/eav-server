//namespace ToSic.Eav.Persistence.Efc.Models;

//[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
//public partial class ToSicEavAttributeGroups
//{
//    public int AttributeGroupId { get; set; }
//    public string Name { get; set; }
        
//    // 2022-12-15 removed from DB / SQL in v15
//    //public int SortOrder { get; set; }
//    public int AttributeSetId { get; set; }

//    public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSets { get; set; } = new HashSet<ToSicEavAttributesInSets>();
//    public virtual ToSicEavAttributeSets AttributeSet { get; set; }
//}