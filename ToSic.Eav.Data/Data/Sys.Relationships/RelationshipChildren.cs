using ToSic.Eav.Data.Sys.Attributes;

namespace ToSic.Eav.Data.Relationships.Sys;

/// <summary>
/// A dictionary-style children accessor containing all fields which have child-entities. <br/>
/// Used on the <see cref="IEntity"/> Children property.
/// </summary>
[PrivateApi("this is for the Relationship.Children API, not recommended for others")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class RelationshipChildren(IReadOnlyDictionary<string, IAttribute> attributes) : IRelationshipChildren
{
    /// <inheritdoc />
    /// <summary>
    /// Get Children of a specified Attribute Name
    /// </summary>
    /// <param name="attributeName">Attribute Name</param>
    public IEnumerable<IEntity> this[string attributeName]
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (attributes == null)
                return [];

            var list = attributes.TryGetValue(attributeName, out var attribute)
                ? (attribute as Attribute<IEnumerable<IEntity>>)?.TypedContents
                : null;

            return list ?? [];
        }
    }
}