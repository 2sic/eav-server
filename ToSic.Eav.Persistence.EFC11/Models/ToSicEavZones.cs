using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavZones//: RepoZone
    {
        public ToSicEavZones()
        {
            ToSicEavApps = new HashSet<ToSicEavApps>();
            ToSicEavDimensions = new HashSet<ToSicEavDimensions>();
        }

        public int ZoneId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ToSicEavApps> ToSicEavApps { get; set; }
        public virtual ICollection<ToSicEavDimensions> ToSicEavDimensions { get; set; }
    }
}
