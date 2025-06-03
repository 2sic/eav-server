namespace ToSic.Eav.Data;

/// <summary>
/// A dictionary-style children accessor containing all fields which have child-entities. <br/>
/// Used on the <see cref="IEntity"/> Children property.
/// </summary>
[PrivateApi("this is for the Relationship.Children API, not recommended for others")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class RelationshipChildren : IRelationshipChildren
{
    private readonly IReadOnlyDictionary<string, IAttribute> _attributes;

    /// <summary>
    /// Initializes a new instance of the Children class.
    /// </summary>
    /// <param name="attributes"></param>
    internal RelationshipChildren(IReadOnlyDictionary<string, IAttribute> attributes)
    {
        _attributes = attributes;
    }

    /// <inheritdoc />
    /// <summary>
    /// Get Children of a specified Attribute Name
    /// </summary>
    /// <param name="attributeName">Attribute Name</param>
    public IEnumerable<IEntity> this[string attributeName]
    {
        get
        {
            if (_attributes == null) return new List<IEntity>();
            return _attributes.TryGetValue(attributeName, out var attribute) 
                ? (attribute as Attribute<IEnumerable<IEntity>>)?.TypedContents 
                : new List<IEntity>();
        }
    }
}