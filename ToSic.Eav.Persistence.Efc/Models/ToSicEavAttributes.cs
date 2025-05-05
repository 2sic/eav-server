namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavAttributes
{
    public int AttributeId { get; set; }

    public string StaticName { get; set; }

    public string Type { get; set; }

    public int TransactionIdCreated { get; set; }

    public int? TransactionIdDeleted { get; set; }

    public Guid? Guid { get; set; }

    public string SysSettings { get; set; }

    public int ContentTypeId { get; set; }

    public int SortOrder { get; set; }

    public bool IsTitle { get; set; } = false;

    public virtual TsDynDataTransaction TransactionCreatedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionDeletedNavigation { get; set; }

    public virtual ICollection<ToSicEavEntityRelationships> ToSicEavEntityRelationships { get; set; } = new HashSet<ToSicEavEntityRelationships>();

    public virtual ICollection<ToSicEavValues> ToSicEavValues { get; set; } = new HashSet<ToSicEavValues>();

    public virtual ToSicEavAttributeTypes TypeNavigation { get; set; }

    public virtual ToSicEavAttributeSets AttributeSet { get; set; }
}