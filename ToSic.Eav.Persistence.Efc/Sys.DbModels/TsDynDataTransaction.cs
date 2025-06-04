namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataTransaction
{
    public int TransactionId { get; set; }

    public DateTime Timestamp { get; set; }

    public string User { get; set; }

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributesTransCreated { get; set; } = new HashSet<TsDynDataAttribute>();

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributesTransModified { get; set; } = new HashSet<TsDynDataAttribute>();

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributesTransDeleted { get; set; } = new HashSet<TsDynDataAttribute>();

    public virtual ICollection<TsDynDataContentType> TsDynDataContentTypesTransCreated { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataContentType> TsDynDataContentTypesTransModified { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataContentType> TsDynDataContentTypesTransDeleted { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntitiesTransCreated { get; set; } = new HashSet<TsDynDataEntity>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntitiesTransDeleted { get; set; } = new HashSet<TsDynDataEntity>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntitiesTransModified { get; set; } = new HashSet<TsDynDataEntity>();

    public virtual ICollection<TsDynDataHistory> TsDynDataHistories { get; set; } = new HashSet<TsDynDataHistory>();

    public virtual ICollection<TsDynDataZone> TsDynDataZonesTransCreated { get; set; } = new HashSet<TsDynDataZone>();
    
    public virtual ICollection<TsDynDataZone> TsDynDataZonesTransModified { get; set; } = new HashSet<TsDynDataZone>();

    public virtual ICollection<TsDynDataZone> TsDynDataZonesTransDeleted { get; set; } = new HashSet<TsDynDataZone>();

    public virtual ICollection<TsDynDataApp> TsDynDataAppsTransCreated { get; set; } = new HashSet<TsDynDataApp>();

    public virtual ICollection<TsDynDataApp> TsDynDataAppsTransModified { get; set; } = new HashSet<TsDynDataApp>();

    public virtual ICollection<TsDynDataApp> TsDynDataAppsTransDeleted { get; set; } = new HashSet<TsDynDataApp>();
}