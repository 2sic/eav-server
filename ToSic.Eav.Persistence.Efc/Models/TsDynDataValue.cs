namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class TsDynDataValue//: RepoValue
{
    public int ValueId { get; set; }

    public int EntityId { get; set; }

    public int AttributeId { get; set; }

    public string Value { get; set; }

    public virtual TsDynDataAttribute Attribute { get; set; }

    public virtual TsDynDataEntity Entity { get; set; }


    public virtual ICollection<ToSicEavValuesDimensions> ToSicEavValuesDimensions { get; set; } = new HashSet<ToSicEavValuesDimensions>();
}