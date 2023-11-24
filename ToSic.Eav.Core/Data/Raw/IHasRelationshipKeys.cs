using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Raw;

/// <summary>
/// Mark objects of type <see cref="IRawEntity"/> to also provide relationship keys.
///
/// This is important to automatically create relationships between newly created <see cref="IEntity"/>s
/// </summary>
/// <remarks>
/// Added in 15.04
/// </remarks>
[PrivateApi("Was public till 16.09")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IHasRelationshipKeys
{
    /// <summary>
    /// The keys this object provides.
    /// Keys can be `int`, `string` or something else.
    /// Note that the keys listed here are keys to which the current object will be returned.
    ///
    /// Example
    /// 
    /// 1. if another <see cref="IRawEntity"/> has a property (eg. `Folders`)
    /// 1. of type <see cref="IRawRelationship"/>/<see cref="RawRelationship"/>
    /// 1. which lists the key `/abcd/efg`
    ///
    /// Then it will ask all other <see cref="IRawEntity"/> of <see cref="IHasRelationshipKeys"/>
    /// if they have a `/abcd/efg` in their `RelationshipKeys`.
    /// </summary>
    IEnumerable<object> RelationshipKeys(RawConvertOptions options);
}