using System;

namespace ToSic.Eav.Data
{
    public partial class EntityWrapper: IEquatable<IEntityWrapper>
    {
        public IEntity EntityForEqualityCheck { get; }

        public static bool operator ==(EntityWrapper d1, IEntityWrapper d2) => EntityWrapperEquality.IsEqual(d1, d2);

        public static bool operator !=(EntityWrapper d1, IEntityWrapper d2) => !EntityWrapperEquality.IsEqual(d1, d2);

        public bool Equals(IEntityWrapper other) => EntityWrapperEquality.IsEqual(this, other);

        public override bool Equals(object obj) => EntityWrapperEquality.EqualsObj(this, obj);

        public override int GetHashCode() => EntityWrapperEquality.GetHashCode(this);
    }
}
