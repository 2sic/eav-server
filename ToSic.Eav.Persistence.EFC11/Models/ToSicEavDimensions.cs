using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavDimensions : Data.DimensionDefinition
    {
        public ToSicEavDimensions()
        {
            ToSicEavValuesDimensions = new HashSet<ToSicEavValuesDimensions>();
        }

        //public int DimensionId { get; set; }
        //public int? Parent { get; set; }
        //public string Name { get; set; }
        //public string Key { get; set; }
        //public string EnvironmentKey { get; set; }
        //public bool Active { get; set; }
        public int ZoneId { get; set; }

        public virtual ICollection<ToSicEavValuesDimensions> ToSicEavValuesDimensions { get; set; }
        public virtual ToSicEavDimensions ParentNavigation { get; set; }
        public virtual ICollection<ToSicEavDimensions> InverseParentNavigation { get; set; }
        public virtual ToSicEavZones Zone { get; set; }
    }
}
