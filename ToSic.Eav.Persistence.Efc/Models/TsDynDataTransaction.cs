namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class TsDynDataTransaction
{
    public int TransactionId { get; set; }

    public DateTime Timestamp { get; set; }

    public string User { get; set; }

    public virtual ICollection<ToSicEavAttributes> ToSicEavAttributesTransactionCreatedNavigation { get; set; } = new HashSet<ToSicEavAttributes>();

    public virtual ICollection<ToSicEavAttributes> ToSicEavAttributesTransactionDeletedNavigation { get; set; } = new HashSet<ToSicEavAttributes>();

    public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSetsTransactionCreatedNavigation { get; set; } = new HashSet<ToSicEavAttributeSets>();

    public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSetsTransactionDeletedNavigation { get; set; } = new HashSet<ToSicEavAttributeSets>();

    public virtual ICollection<ToSicEavEntities> ToSicEavEntitiesTransactionCreatedNavigation { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual ICollection<ToSicEavEntities> ToSicEavEntitiesTransactionDeletedNavigation { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual ICollection<ToSicEavEntities> ToSicEavEntitiesTransactionModifiedNavigation { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual ICollection<ToSicEavValues> ToSicEavValuesTransactionCreatedNavigation { get; set; } = new HashSet<ToSicEavValues>();

    public virtual ICollection<ToSicEavValues> ToSicEavValuesTransactionDeletedNavigation { get; set; } = new HashSet<ToSicEavValues>();

    public virtual ICollection<ToSicEavValues> ToSicEavValuesTransactionModifiedNavigation { get; set; } = new HashSet<ToSicEavValues>();

    public virtual ICollection<TsDynDataHistory> TsDynDataHistories { get; set; } = new HashSet<TsDynDataHistory>();

    public virtual ICollection<TsDynDataZone> TsDynDataZonesTransactionCreatedNavigation { get; set; } = new HashSet<TsDynDataZone>();
    
    public virtual ICollection<TsDynDataZone> TsDynDataZonesTransactionModifiedNavigation { get; set; } = new HashSet<TsDynDataZone>();

    public virtual ICollection<TsDynDataZone> TsDynDataZonesTransactionDeletedNavigation { get; set; } = new HashSet<TsDynDataZone>();

    public virtual ICollection<TsDynDataApp> TsDynDataAppsTransactionCreatedNavigation { get; set; } = new HashSet<TsDynDataApp>();

    public virtual ICollection<TsDynDataApp> TsDynDataAppsTransactionModifiedNavigation { get; set; } = new HashSet<TsDynDataApp>();

    public virtual ICollection<TsDynDataApp> TsDynDataAppsTransactionDeletedNavigation { get; set; } = new HashSet<TsDynDataApp>();
}