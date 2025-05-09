using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.TreeMapperTests;

internal class RawItemWithOneParentAndManyChildren(int id, Guid guid, int parentId, List<int>? childrenIds)
    : IRawEntity, IHasRelationshipKeys
{
    public int Id { get;  } = id;
    public Guid Guid { get;  } = guid;
    public DateTime Created { get; } = DateTime.Now;
    public DateTime Modified { get; } = DateTime.Now;

    public string Title => $"Auto-Title {Id} / {Guid}";

    public int ParentId { get; } = parentId;

    public List<int>? ChildrenIds { get; } = childrenIds;

    public IDictionary<string, object> Attributes(RawConvertOptions options) => new Dictionary<string, object>
    {
        { nameof(Title), Title },
        { "Children", new RawRelationship(keys: ChildrenIds?.Cast<object>() ?? new List<object>())},
    };

    public IEnumerable<object> RelationshipKeys(RawConvertOptions options) => new List<object> { Id };
}