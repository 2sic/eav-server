namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal record RelationshipToSave
{
    public required int AttributeId { get; init; }
    public ICollection<Guid?>? ChildEntityGuids { get; init; }
    public ICollection<int?>? ChildEntityIds { get; init; }

    public required int ParentEntityId { get; init; }


    public required bool FlushAllEntityRelationships { get; init; }
}