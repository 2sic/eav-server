namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavEntityRelationships
{
    public int AttributeId { get; set; }

    public int ParentEntityId { get; set; }

    public int? ChildEntityId { get; set; }

    public int SortOrder { get; set; }

    public virtual TsDynDataAttribute Attribute { get; set; }

    public virtual TsDynDataEntity ChildEntity { get; set; }

    public virtual TsDynDataEntity ParentEntity { get; set; }
}