namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavEntityRelationships
    {
        public int AttributeId { get; set; }
        public int ParentEntityId { get; set; }
        public int? ChildEntityId { get; set; }
        public int SortOrder { get; set; }

        public virtual ToSicEavAttributes Attribute { get; set; }
        public virtual ToSicEavEntities ChildEntity { get; set; }
        public virtual ToSicEavEntities ParentEntity { get; set; }
    }
}
