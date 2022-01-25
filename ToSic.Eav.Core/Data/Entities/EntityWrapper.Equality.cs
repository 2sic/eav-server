using System;

namespace ToSic.Eav.Data
{
    public partial class EntityWrapper: IEquatable<IEntityWrapper>, IEquatable<IMultiWrapper<IEntity>>
    {
        public IEntity RootContentsForEqualityCheck { get; }

        public static bool operator ==(EntityWrapper d1, IEntityWrapper d2) => WrapperEquality.IsEqual(d1, d2);

        public static bool operator !=(EntityWrapper d1, IEntityWrapper d2) => !WrapperEquality.IsEqual(d1, d2);

        public static bool operator ==(EntityWrapper d1, IMultiWrapper<IEntity> d2) => WrapperEquality.IsEqual(d1, d2);

        public static bool operator !=(EntityWrapper d1, IMultiWrapper<IEntity> d2) => !WrapperEquality.IsEqual(d1, d2);


        public bool Equals(IEntityWrapper other) => WrapperEquality.IsEqual(this, other);

        public bool Equals(IMultiWrapper<IEntity> other) => WrapperEquality.IsEqual(this, other);

        public override bool Equals(object other) => WrapperEquality.EqualsObj(this, other);

        public override int GetHashCode() => WrapperEquality.GetHashCode(this);
    }
}
