namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavEntities
{
    public int EntityId { get; set; }

    public Guid EntityGuid { get; set; }

    public int AttributeSetId { get; set; }

    // 2017-06-20 2dm - never used
    //public int? ConfigurationSet { get; set; }

    public int TargetTypeId { get; set; }

    public int? KeyNumber { get; set; }

    public Guid? KeyGuid { get; set; }

    public string KeyString { get; set; }

    public int TransactionIdCreated { get; set; }

    public int? TransactionIdDeleted { get; set; }

    public bool IsPublished { get; set; }

    public int? PublishedEntityId { get; set; }

    public int TransactionIdModified { get; set; }

    public string Owner { get; set; }

    public string Json { get; set; }

    public int Version { get; set; } = 1;

    public int AppId { get; set; }

    public string ContentType { get; set; }

    // 2017-10-10 2dm new with entity > app mapping
    public virtual TsDynDataApp App { get; set; }

    public virtual TsDynDataTargetType TargetType { get; set; }

    public virtual ToSicEavAttributeSets AttributeSet { get; set; }

    public virtual TsDynDataTransaction TransactionCreatedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionDeletedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionModifiedNavigation { get; set; }

    //public virtual ToSicEavEntities ConfigurationSetNavigation { get; set; }

    //public virtual ICollection<ToSicEavEntities> InverseConfigurationSetNavigation { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual ICollection<ToSicEavEntityRelationships> RelationshipsWithThisAsChild { get; set; } = new HashSet<ToSicEavEntityRelationships>();

    public virtual ICollection<ToSicEavEntityRelationships> RelationshipsWithThisAsParent { get; set; } = new HashSet<ToSicEavEntityRelationships>();

    public virtual ICollection<ToSicEavValues> ToSicEavValues { get; set; } = new HashSet<ToSicEavValues>();
}