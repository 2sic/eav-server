namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavValues//: RepoValue
{
    public ToSicEavValues()
    {
        ToSicEavValuesDimensions = new HashSet<ToSicEavValuesDimensions>();
    }

    public int ValueId { get; set; }
    public int EntityId { get; set; }
    public int AttributeId { get; set; }
    public string Value { get; set; }
    public int ChangeLogCreated { get; set; }
    public int? ChangeLogDeleted { get; set; }
    public int? ChangeLogModified { get; set; }

    public virtual ToSicEavAttributes Attribute { get; set; }
    public virtual ToSicEavChangeLog ChangeLogCreatedNavigation { get; set; }
    public virtual ToSicEavChangeLog ChangeLogDeletedNavigation { get; set; }
    public virtual ToSicEavChangeLog ChangeLogModifiedNavigation { get; set; }
    public virtual ToSicEavEntities Entity { get; set; }
    public virtual ICollection<ToSicEavValuesDimensions> ToSicEavValuesDimensions { get; set; }
}