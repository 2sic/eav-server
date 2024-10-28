namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavChangeLog
{
    public int ChangeId { get; set; }

    public DateTime Timestamp { get; set; }

    public string User { get; set; }

    public virtual ICollection<ToSicEavAttributes> ToSicEavAttributesChangeLogCreatedNavigation { get; set; } = new HashSet<ToSicEavAttributes>();

    public virtual ICollection<ToSicEavAttributes> ToSicEavAttributesChangeLogDeletedNavigation { get; set; } = new HashSet<ToSicEavAttributes>();

    public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSetsChangeLogCreatedNavigation { get; set; } = new HashSet<ToSicEavAttributeSets>();

    public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSetsChangeLogDeletedNavigation { get; set; } = new HashSet<ToSicEavAttributeSets>();

    public virtual ICollection<ToSicEavEntities> ToSicEavEntitiesChangeLogCreatedNavigation { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual ICollection<ToSicEavEntities> ToSicEavEntitiesChangeLogDeletedNavigation { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual ICollection<ToSicEavEntities> ToSicEavEntitiesChangeLogModifiedNavigation { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual ICollection<ToSicEavValues> ToSicEavValuesChangeLogCreatedNavigation { get; set; } = new HashSet<ToSicEavValues>();

    public virtual ICollection<ToSicEavValues> ToSicEavValuesChangeLogDeletedNavigation { get; set; } = new HashSet<ToSicEavValues>();

    public virtual ICollection<ToSicEavValues> ToSicEavValuesChangeLogModifiedNavigation { get; set; } = new HashSet<ToSicEavValues>();
}