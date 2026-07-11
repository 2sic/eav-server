using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.TreeMapperTests;

internal record RawItemWithOneParentAndManyChildren(int Id, Guid Guid, int ParentId, List<int>? ChildrenIds)
    : IRawEntity, IHasRelationshipKeys
{
    public DateTime Created { get; } = DateTime.Now;
    public DateTime Modified { get; } = DateTime.Now;

    public string Title => $"Auto-Title {Id} / {Guid}";

    IDictionary<string, object?> IRawEntity.Values => field ??= new Dictionary<string, object?>
    {
        { nameof(Title), Title },
        { "Children", new RawRelationship(keys: ChildrenIds?.Cast<object>() ?? new List<object>()) },
    };

    public IEnumerable<object> RelationshipKeys => new List<object> { Id };

    IConvertToRawEntity? IGetRawConverter.GetConverter() => null;

}