using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Raw;

/// <summary>
/// Marks raw relationships.
/// This is internal, only used to mark <see cref="RawRelationship"/> objects which are generic.
/// </summary>
/// <remarks>
/// Added in 15.04
/// </remarks>
[PrivateApi("was internal till 16.09")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IRawRelationship
{
}