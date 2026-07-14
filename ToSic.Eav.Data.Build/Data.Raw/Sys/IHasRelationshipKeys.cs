namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// Mark objects of type <see cref="IRawEntity"/> to also provide relationship keys.
///
/// This is important to automatically create relationships between newly created <see cref="IEntity"/>s
/// </summary>
/// <remarks>
/// * Added in 15.04, accidentally public
/// * Was public till 16.09, but needed to be reworked, made private
/// * v22 made the property nullable, so it can be null if no keys are provided
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasRelationshipKeys
{
    /// <summary>
    /// Optional relationship keys - can also be `null` if never specified.
    /// 
    /// The keys this object provides - meaning a reference to such a key should point to this object.
    /// Keys can be `int`, `string` or something else.
    ///
    /// Example
    /// 
    /// 1. if another <see cref="IRawEntity"/> has a property (like `Folders`)
    /// 1. of type <see cref="IRawRelationship"/>/<see cref="RawRelationship"/>
    /// 1. which lists the key `/abcd/efg`
    ///
    /// Then it will ask all other <see cref="IRawEntity"/> of <see cref="IHasRelationshipKeys"/>
    /// if they have a `/abcd/efg` in their `RelationshipKeys`.
    /// </summary>
    IEnumerable<object>? RelationshipKeys { get; }
}