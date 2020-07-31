using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavAttributeGroups
    {
        public ToSicEavAttributeGroups()
        {
            ToSicEavAttributesInSets = new HashSet<ToSicEavAttributesInSets>();
        }

        public int AttributeGroupId { get; set; }
        public string Name { get; set; }
        // 2020-07-31 2dm removed, never used
        //public int SortOrder { get; set; }
        public int AttributeSetId { get; set; }

        public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSets { get; set; }
        public virtual ToSicEavAttributeSets AttributeSet { get; set; }
    }
}
