namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavAttributes
{
    public ToSicEavAttributes()
    {
        ToSicEavAttributesInSets = new HashSet<ToSicEavAttributesInSets>();
        ToSicEavEntityRelationships = new HashSet<ToSicEavEntityRelationships>();
        ToSicEavValues = new HashSet<ToSicEavValues>();
    }

    public int AttributeId { get; set; }
    public string StaticName { get; set; }
    public string Type { get; set; }
    public int ChangeLogCreated { get; set; }
    public int? ChangeLogDeleted { get; set; }
    public Guid? Guid { get; set; }
    public string SysSettings { get; set; }

    public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSets { get; set; }
    public virtual ICollection<ToSicEavEntityRelationships> ToSicEavEntityRelationships { get; set; }
    public virtual ICollection<ToSicEavValues> ToSicEavValues { get; set; }
    public virtual ToSicEavChangeLog ChangeLogCreatedNavigation { get; set; }
    public virtual ToSicEavChangeLog ChangeLogDeletedNavigation { get; set; }
    public virtual ToSicEavAttributeTypes TypeNavigation { get; set; }
}