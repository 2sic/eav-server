namespace ToSic.Eav.Persistence.Efc.Models;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataZone//: RepoZone
{
    public int ZoneId { get; set; }

    public string Name { get; set; }

    public int? TransCreatedId { get; set; }

    public int? TransModifiedId { get; set; }

    public int? TransDeletedId { get; set; }

    public virtual ICollection<TsDynDataApp> TsDynDataApps { get; set; } = new HashSet<TsDynDataApp>();

    public virtual ICollection<TsDynDataDimension> TsDynDataDimensions { get; set; } =  new HashSet<TsDynDataDimension>();

    public virtual TsDynDataTransaction TransCreated { get; set; }

    public virtual TsDynDataTransaction TransModified { get; set; }

    public virtual TsDynDataTransaction TransDeleted { get; set; }
}