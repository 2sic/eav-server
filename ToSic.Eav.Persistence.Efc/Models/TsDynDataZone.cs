namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class TsDynDataZone//: RepoZone
{
    public int ZoneId { get; set; }

    public string Name { get; set; }

    public int? TransactionIdCreated { get; set; }

    public int? TransactionIdModified { get; set; }

    public int? TransactionIdDeleted { get; set; }

    public virtual ICollection<TsDynDataApp> TsDynDataApps { get; set; } = new HashSet<TsDynDataApp>();

    public virtual ICollection<TsDynDataDimension> TsDynDataDimensions { get; set; } =  new HashSet<TsDynDataDimension>();

    public virtual TsDynDataTransaction TransactionCreatedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionModifiedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionDeletedNavigation { get; set; }
}