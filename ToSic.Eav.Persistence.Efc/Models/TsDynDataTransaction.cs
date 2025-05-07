namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class TsDynDataTransaction
{
    public int TransactionId { get; set; }

    public DateTime Timestamp { get; set; }

    public string User { get; set; }

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributesTransactionCreatedNavigation { get; set; } = new HashSet<TsDynDataAttribute>();

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributesTransactionModifiedNavigation { get; set; } = new HashSet<TsDynDataAttribute>();

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributesTransactionDeletedNavigation { get; set; } = new HashSet<TsDynDataAttribute>();

    public virtual ICollection<TsDynDataContentType> TsDynDataContentTypesTransactionCreatedNavigation { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataContentType> TsDynDataContentTypesTransactionModifiedNavigation { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataContentType> TsDynDataContentTypesTransactionDeletedNavigation { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntitiesTransactionCreatedNavigation { get; set; } = new HashSet<TsDynDataEntity>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntitiesTransactionDeletedNavigation { get; set; } = new HashSet<TsDynDataEntity>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntitiesTransactionModifiedNavigation { get; set; } = new HashSet<TsDynDataEntity>();

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