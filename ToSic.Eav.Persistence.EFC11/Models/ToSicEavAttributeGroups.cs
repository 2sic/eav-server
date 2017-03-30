using System;
using System.Collections.Generic;

namespace ToSic.Eav.Persistence.EFC11.Models
{
    public partial class ToSicEavAttributeGroups
    {
        public ToSicEavAttributeGroups()
        {
            ToSicEavAttributesInSets = new HashSet<ToSicEavAttributesInSets>();
        }

        public int AttributeGroupId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public int AttributeSetId { get; set; }

        public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSets { get; set; }
        public virtual ToSicEavAttributeSets AttributeSet { get; set; }
    }
}
