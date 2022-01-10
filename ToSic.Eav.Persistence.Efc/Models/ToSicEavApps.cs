using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavApps//: RepoApp
    {
        public ToSicEavApps()
        {
            ToSicEavAttributeSets = new HashSet<ToSicEavAttributeSets>();

            // 2017-10-10 2dm new with entity > app mapping
            ToSicEavEntities = new HashSet<ToSicEavEntities>();
        }

        public int AppId { get; set; }
        public int ZoneId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSets { get; set; }
        public virtual ToSicEavZones Zone { get; set; }

        // 2017-10-10 2dm new with entity > app mapping
        public virtual ICollection<ToSicEavEntities> ToSicEavEntities { get; set; }

        public string SysSettings { get; set; } = null;
    }
}
