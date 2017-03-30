using System;
using System.Collections.Generic;

namespace ToSic.Eav.Persistence.EFC11.Models
{
    public partial class ToSicEavAttributeTypes
    {
        public ToSicEavAttributeTypes()
        {
            ToSicEavAttributes = new HashSet<ToSicEavAttributes>();
        }

        public string Type { get; set; }

        public virtual ICollection<ToSicEavAttributes> ToSicEavAttributes { get; set; }
    }
}
