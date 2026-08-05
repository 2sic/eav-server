using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Raw;

/// <summary>
/// Represents a relationship in a RawEntity.
///
/// Basically you add such an object to your properties dictionary, containing the keys it needs to find its related items.
/// </summary>
/// <remarks>
/// * Added in 15.04, accidentally public
/// * Made private again in v16.09 as we see reasons to improve
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record RawRelationship : IRawRelationship
{
    /// <summary>
    /// This is the property name used on anonymous objects to designate a relationship.
    /// For example, if you have `data = new { Children = new { Relationships = "File/472" } }`
    /// so if you have a property called `Relationships` on your anonymous property, it will be recognized as a relationship.
    /// </summary>
    public const string RelationshipsKey = "Relationships";

    /// <summary>
    /// The keys which will be used to find the related items.
    /// </summary>
    public List<object> Keys { get; init; } = [];
}