namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataAttributeType
{
    public string Type { get; set; }

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributes { get; set; } = new HashSet<TsDynDataAttribute>();
}