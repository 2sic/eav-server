﻿namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavEntities
{
    public ToSicEavEntities()
    {
        RelationshipsWithThisAsChild = new HashSet<ToSicEavEntityRelationships>();
        RelationshipsWithThisAsParent = new HashSet<ToSicEavEntityRelationships>();
        ToSicEavValues = new HashSet<ToSicEavValues>();
    }

    public int AppId { get; set; }

    public int EntityId { get; set; }
    public Guid EntityGuid { get; set; }
    public int AttributeSetId { get; set; }
    // 2017-06-20 2dm - never used
    //public int? ConfigurationSet { get; set; }
    public int AssignmentObjectTypeId { get; set; }
    public int? KeyNumber { get; set; }
    public Guid? KeyGuid { get; set; }
    public string KeyString { get; set; }
        
    // 2022-12-15 removed from DB / SQL in v15
    //public int SortOrder { get; set; } = 0;

    public int ChangeLogCreated { get; set; }
    public int? ChangeLogDeleted { get; set; }
    public bool IsPublished { get; set; }
    public int? PublishedEntityId { get; set; }
    public int ChangeLogModified { get; set; }
    public string Owner { get; set; }

    public string ContentType { get; set; }

    public virtual ICollection<ToSicEavEntityRelationships> RelationshipsWithThisAsChild { get; set; }
    public virtual ICollection<ToSicEavEntityRelationships> RelationshipsWithThisAsParent { get; set; }
    public virtual ICollection<ToSicEavValues> ToSicEavValues { get; set; }
    public virtual ToSicEavAssignmentObjectTypes AssignmentObjectType { get; set; }
    public virtual ToSicEavAttributeSets AttributeSet { get; set; }
    public virtual ToSicEavChangeLog ChangeLogCreatedNavigation { get; set; }
    public virtual ToSicEavChangeLog ChangeLogDeletedNavigation { get; set; }
    public virtual ToSicEavChangeLog ChangeLogModifiedNavigation { get; set; }

    // 2017-10-10 2dm new with entity > app mapping
    public virtual ToSicEavApps App { get; set; }
    //public virtual ToSicEavEntities ConfigurationSetNavigation { get; set; }
    //public virtual ICollection<ToSicEavEntities> InverseConfigurationSetNavigation { get; set; }

    public string Json { get; set; }
    public int Version { get; set; } = 1;
}