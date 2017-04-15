using System;
using System.Collections.Generic;

namespace ToSic.Eav.Persistence.EFC11.Models
{
    public partial class ToSicEavEntities
    {
        public ToSicEavEntities()
        {
            RelationshipsWithThisAsChild = new HashSet<ToSicEavEntityRelationships>();
            RelationshipsWithThisAsParent = new HashSet<ToSicEavEntityRelationships>();
            ToSicEavValues = new HashSet<ToSicEavValues>();
        }

        public int EntityId { get; set; }
        public Guid EntityGuid { get; set; }
        public int AttributeSetId { get; set; }
        public int? ConfigurationSet { get; set; }
        public int AssignmentObjectTypeId { get; set; }
        public int? KeyNumber { get; set; }
        public Guid? KeyGuid { get; set; }
        public string KeyString { get; set; }
        public int SortOrder { get; set; }
        public int ChangeLogCreated { get; set; }
        public int? ChangeLogDeleted { get; set; }
        public bool IsPublished { get; set; }
        public int? PublishedEntityId { get; set; }
        public int ChangeLogModified { get; set; }
        public string Owner { get; set; }

        public virtual ICollection<ToSicEavEntityRelationships> RelationshipsWithThisAsChild { get; set; }
        public virtual ICollection<ToSicEavEntityRelationships> RelationshipsWithThisAsParent { get; set; }
        public virtual ICollection<ToSicEavValues> ToSicEavValues { get; set; }
        public virtual ToSicEavAssignmentObjectTypes AssignmentObjectType { get; set; }
        public virtual ToSicEavAttributeSets AttributeSet { get; set; }
        public virtual ToSicEavChangeLog ChangeLogCreatedNavigation { get; set; }
        public virtual ToSicEavChangeLog ChangeLogDeletedNavigation { get; set; }
        public virtual ToSicEavChangeLog ChangeLogModifiedNavigation { get; set; }
        public virtual ToSicEavEntities ConfigurationSetNavigation { get; set; }
        public virtual ICollection<ToSicEavEntities> InverseConfigurationSetNavigation { get; set; }
    }
}
