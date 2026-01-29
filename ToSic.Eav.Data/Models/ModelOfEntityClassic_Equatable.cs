namespace ToSic.Eav.Models;

partial class ModelOfEntityClassic: IMultiWrapper<IEntity>, IEquatable<IEntity>
{
    bool IEquatable<IEntity>.Equals(IEntity? other) => Equals(other);

    /// <summary>
    /// Ensure that the equality check is done correctly.
    /// If two objects wrap the same item, they will be considered equal.
    /// </summary>
    public override bool Equals(object? b)
        => MultiWrapperEquality.EqualsObj(this, b);

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public override int GetHashCode()
        => MultiWrapperEquality.GetWrappedHashCode(this);

    IEntity? IMultiWrapper<IEntity>.RootContentsForEqualityCheck
        => (Entity as IMultiWrapper<IEntity>)?.RootContentsForEqualityCheck;

    /// <summary>
    /// Ensure that the equality check is done correctly.
    /// If two objects wrap the same item, they will be considered equal.
    /// </summary>
    /// <param name="a">first item to compare</param>
    /// <param name="b">second item to compare</param>
    /// <returns>true, if both wrappers are the same type and wrap the same entity</returns>
    public static bool operator ==(ModelOfEntityClassic a, ModelOfEntityClassic b)
        => MultiWrapperEquality.IsEqual(a, b);

    /// <summary>
    /// Ensure that the equality check is done correctly.
    /// If two objects wrap the same item, they will be considered equal.
    /// </summary>
    /// <param name="item1">first item to compare</param>
    /// <param name="item2">second item to compare</param>
    /// <returns>false, if both wrappers are the same type and wrap the same entity</returns>
    public static bool operator !=(ModelOfEntityClassic item1, ModelOfEntityClassic item2)
        => !MultiWrapperEquality.IsEqual(item1, item2);
}