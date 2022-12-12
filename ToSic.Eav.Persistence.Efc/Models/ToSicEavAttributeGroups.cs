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
        // 2020-07-31 2dm can't remove even though, never used - otherwise it tries to use a null-value
        // so we can't remove until we change the DB / SQL, which we don't want to do soon
        // TODO: @STV remove in SQL-Update whenever we do this next
        public int SortOrder { get; set; }
        public int AttributeSetId { get; set; }

        public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSets { get; set; }
        public virtual ToSicEavAttributeSets AttributeSet { get; set; }
    }
}
