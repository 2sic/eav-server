using System;
using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavApps//: RepoApp
    {
        public ToSicEavApps()
        {
            ToSicEavAttributeSets = new HashSet<ToSicEavAttributeSets>();
        }

        public int AppId { get; set; }
        public int ZoneId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSets { get; set; }
        public virtual ToSicEavZones Zone { get; set; }
    }
}
