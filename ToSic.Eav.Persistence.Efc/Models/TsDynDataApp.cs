namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class TsDynDataApp
{
    public int AppId { get; set; }

    public int ZoneId { get; set; }

    public string Name { get; set; }

    public string SysSettings { get; set; } = null;

    public int? TransactionIdCreated { get; set; }

    public int? TransactionIdModified { get; set; }

    public int? TransactionIdDeleted { get; set; }

    public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSets { get; set; } = new HashSet<ToSicEavAttributeSets>();

    public virtual ICollection<ToSicEavEntities> ToSicEavEntities { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual TsDynDataZone Zone { get; set; }

    public virtual TsDynDataTransaction TransactionCreatedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionModifiedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionDeletedNavigation { get; set; }
}