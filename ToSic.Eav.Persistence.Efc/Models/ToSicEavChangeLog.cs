using System;
using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavChangeLog
    {
        public ToSicEavChangeLog()
        {
            ToSicEavAttributesChangeLogCreatedNavigation = new HashSet<ToSicEavAttributes>();
            ToSicEavAttributesChangeLogDeletedNavigation = new HashSet<ToSicEavAttributes>();
            ToSicEavAttributeSetsChangeLogCreatedNavigation = new HashSet<ToSicEavAttributeSets>();
            ToSicEavAttributeSetsChangeLogDeletedNavigation = new HashSet<ToSicEavAttributeSets>();
            ToSicEavEntitiesChangeLogCreatedNavigation = new HashSet<ToSicEavEntities>();
            ToSicEavEntitiesChangeLogDeletedNavigation = new HashSet<ToSicEavEntities>();
            ToSicEavEntitiesChangeLogModifiedNavigation = new HashSet<ToSicEavEntities>();
            ToSicEavValuesChangeLogCreatedNavigation = new HashSet<ToSicEavValues>();
            ToSicEavValuesChangeLogDeletedNavigation = new HashSet<ToSicEavValues>();
            ToSicEavValuesChangeLogModifiedNavigation = new HashSet<ToSicEavValues>();
        }

        public int ChangeId { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }

        public virtual ICollection<ToSicEavAttributes> ToSicEavAttributesChangeLogCreatedNavigation { get; set; }
        public virtual ICollection<ToSicEavAttributes> ToSicEavAttributesChangeLogDeletedNavigation { get; set; }
        public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSetsChangeLogCreatedNavigation { get; set; }
        public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSetsChangeLogDeletedNavigation { get; set; }
        public virtual ICollection<ToSicEavEntities> ToSicEavEntitiesChangeLogCreatedNavigation { get; set; }
        public virtual ICollection<ToSicEavEntities> ToSicEavEntitiesChangeLogDeletedNavigation { get; set; }
        public virtual ICollection<ToSicEavEntities> ToSicEavEntitiesChangeLogModifiedNavigation { get; set; }
        public virtual ICollection<ToSicEavValues> ToSicEavValuesChangeLogCreatedNavigation { get; set; }
        public virtual ICollection<ToSicEavValues> ToSicEavValuesChangeLogDeletedNavigation { get; set; }
        public virtual ICollection<ToSicEavValues> ToSicEavValuesChangeLogModifiedNavigation { get; set; }
    }
}
