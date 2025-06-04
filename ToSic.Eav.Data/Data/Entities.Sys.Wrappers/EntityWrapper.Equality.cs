using ToSic.Lib.Wrappers;

namespace ToSic.Eav.Data.Entities.Sys.Wrappers;

partial class EntityWrapper: IEquatable<IEntityWrapper>, IEquatable<IMultiWrapper<IEntity>>
{
    public IEntity RootContentsForEqualityCheck { get; }

    public static bool operator ==(EntityWrapper d1, IEntityWrapper d2) => MultiWrapperEquality.IsEqual(d1, d2);

    public static bool operator !=(EntityWrapper d1, IEntityWrapper d2) => !MultiWrapperEquality.IsEqual(d1, d2);

    public static bool operator ==(EntityWrapper d1, IMultiWrapper<IEntity> d2) => MultiWrapperEquality.IsEqual(d1, d2);

    public static bool operator !=(EntityWrapper d1, IMultiWrapper<IEntity> d2) => !MultiWrapperEquality.IsEqual(d1, d2);


    public bool Equals(IEntityWrapper other) => MultiWrapperEquality.IsEqual(this, other);

    public bool Equals(IMultiWrapper<IEntity> other) => MultiWrapperEquality.IsEqual(this, other);

    public override bool Equals(object other) => MultiWrapperEquality.EqualsObj(this, other);

    public override int GetHashCode() => MultiWrapperEquality.GetWrappedHashCode(this);
}