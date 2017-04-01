using System;
using System.Collections.Generic;
using ToSic.Eav.Repository.Models;

namespace ToSic.Eav.Persistence.EFC11.Models
{
    public partial class ToSicEavZones: RepoZone
    {
        public ToSicEavZones()
        {
            ToSicEavApps = new HashSet<ToSicEavApps>();
            ToSicEavDimensions = new HashSet<ToSicEavDimensions>();
        }

        //public int ZoneId { get; set; }
        //public string Name { get; set; }

        public virtual ICollection<ToSicEavApps> ToSicEavApps { get; set; }
        public virtual ICollection<ToSicEavDimensions> ToSicEavDimensions { get; set; }
    }
}
