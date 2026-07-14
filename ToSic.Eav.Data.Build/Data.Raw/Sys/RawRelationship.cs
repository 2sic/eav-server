namespace ToSic.Eav.Data.Raw.Sys;

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
public class RawRelationship : IRawRelationship
{
    /// <summary>
    /// This is the property name used on anonymous objects to designate a relationship.
    /// For example, if you have `data = new { Children = new { Relationships = "File/472" } }`
    /// so if you have a property called `Relationships` on your anonymous property, it will be recognized as a relationship.
    /// </summary>
    public const string RelationshipsKey = "Relationships";

    /// <summary>
    /// Create a raw relationship.
    /// </summary>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="key">A single key - if it's just a simple `string`, `int`, etc.</param>
    /// <param name="keys">A list of keys, if you have many.</param>
    public RawRelationship(
        NoParamOrder npo = default,
        object? key = default,
        IEnumerable<object>? keys = default)
    {
        Keys = keys?.ToList()
               ?? (key == null ? null : new List<object> { key })
               ?? [];
    }

    /// <summary>
    /// The keys which will be used to find the related items.
    /// </summary>
    public List<object> Keys { get; }
}