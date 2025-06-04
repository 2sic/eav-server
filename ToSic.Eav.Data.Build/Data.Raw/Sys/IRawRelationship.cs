namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// Marks raw relationships.
/// This is internal, only used to mark <see cref="RawRelationship"/> objects which are generic.
/// </summary>
/// <remarks>
/// Added in 15.04
/// </remarks>
[PrivateApi("was internal till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRawRelationship;