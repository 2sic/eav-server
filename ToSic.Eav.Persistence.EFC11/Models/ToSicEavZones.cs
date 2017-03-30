using System;
using System.Collections.Generic;

namespace ToSic.Eav.Persistence.EFC11.Models
{
    public partial class ToSicEavZones
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
