namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class TsDynDataAttribute
{
    public int AttributeId { get; set; }

    public string StaticName { get; set; }

    public string Type { get; set; }

    public Guid? Guid { get; set; }

    public string SysSettings { get; set; }

    public int ContentTypeId { get; set; }

    public int SortOrder { get; set; }

    public bool IsTitle { get; set; } = false;

    public int TransactionIdCreated { get; set; }

    public int? TransactionIdModified { get; set; }

    public int? TransactionIdDeleted { get; set; }

    public virtual TsDynDataAttributeType TypeNavigation { get; set; }

    public virtual TsDynDataContentType ContentType { get; set; }

    public virtual TsDynDataTransaction TransactionCreatedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionModifiedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionDeletedNavigation { get; set; }

    public virtual ICollection<TsDynDataRelationship> TsDynDataRelationships { get; set; } = new HashSet<TsDynDataRelationship>();

    public virtual ICollection<TsDynDataValue> TsDynDataValues { get; set; } = new HashSet<TsDynDataValue>();
}