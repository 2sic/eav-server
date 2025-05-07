namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class TsDynDataEntity
{
    public int EntityId { get; set; }

    public Guid EntityGuid { get; set; }

    public int ContentTypeId { get; set; }

    public int TargetTypeId { get; set; }

    public int? KeyNumber { get; set; }

    public Guid? KeyGuid { get; set; }

    public string KeyString { get; set; }

    public bool IsPublished { get; set; }

    public int? PublishedEntityId { get; set; }

    public string Owner { get; set; }

    public string Json { get; set; }

    public int Version { get; set; } = 1;

    public int AppId { get; set; }

    public string ContentType { get; set; }

    public int TransactionIdCreated { get; set; }

    public int TransactionIdModified { get; set; }

    public int? TransactionIdDeleted { get; set; }

    // 2017-10-10 2dm new with entity > app mapping
    public virtual TsDynDataApp App { get; set; }

    public virtual TsDynDataTargetType TargetType { get; set; }

    public virtual TsDynDataContentType ContentTypeNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionCreatedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionDeletedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionModifiedNavigation { get; set; }

    public virtual ICollection<TsDynDataRelationship> RelationshipsWithThisAsChild { get; set; } = new HashSet<TsDynDataRelationship>();

    public virtual ICollection<TsDynDataRelationship> RelationshipsWithThisAsParent { get; set; } = new HashSet<TsDynDataRelationship>();

    public virtual ICollection<TsDynDataValue> TsDynDataValues { get; set; } = new HashSet<TsDynDataValue>();
}