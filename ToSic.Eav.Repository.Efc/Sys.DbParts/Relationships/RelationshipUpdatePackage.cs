namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal record RelationshipUpdatePackage
{
    public required int AttributeId { get; init; }
    public required ICollection<int?> Targets { get; init; }

    /// <summary>
    /// This is just a stub with EntityId, but also MUST have the `RelationshipsWithThisAsParent` filled
    /// If future code needs it to be filled more, make sure it's constructed that way before.
    /// </summary>
    public required TsDynDataEntity EntityStubWithChildren { get; init; }
}