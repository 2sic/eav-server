using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSourceTests.TreeMapperTests;

internal class RawItemWithOneParentAndManyChildren: IRawEntity, IHasRelationshipKeys
{
    public RawItemWithOneParentAndManyChildren(int id, Guid guid, int parentId, List<int> childrenIds)
    {
        Id = id;
        Guid = guid;
        Created = DateTime.Now;
        Modified = DateTime.Now;
        ParentId = parentId;
        ChildrenIds = childrenIds;
    }

    public int Id { get;  }
    public Guid Guid { get;  }
    public DateTime Created { get; }
    public DateTime Modified { get; }

    public string Title => $"Auto-Title {Id} / {Guid}";

    public int ParentId { get; }

    public List<int> ChildrenIds { get; }

    public IDictionary<string, object> Attributes(RawConvertOptions options) => new Dictionary<string, object>
    {
        { nameof(Title), Title },
        { "Children", new RawRelationship(keys: ChildrenIds?.Cast<object>() ?? new List<object>())},
    };

    public IEnumerable<object> RelationshipKeys(RawConvertOptions options) => new List<object> { Id };
}