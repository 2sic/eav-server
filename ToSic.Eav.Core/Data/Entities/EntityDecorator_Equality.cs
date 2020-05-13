using System;

namespace ToSic.Eav.Data
{
    public partial class EntityDecorator: IEquatable<EntityDecorator>, IEquatable<IEntity>
    {

        public static bool operator ==(EntityDecorator d1, EntityDecorator d2) => IsEqual(d1, d2);

        public static bool operator !=(EntityDecorator d1, EntityDecorator d2) => !IsEqual(d1, d2);


        /// <summary>
        /// Check if they are equal, based on the underlying entity. 
        /// </summary>
        /// <remarks>
        /// It's important to do null-checks first, because if anything in here is null, it will otherwise throw an error. 
        /// But we can't use != null, because that would call the != operator and be recursive.
        /// </remarks>
        /// <returns></returns>
        private static bool IsEqual(EntityDecorator d1, EntityDecorator d2)
            => !(d1 is null) && (ReferenceEquals(d1, d2) || Equals(d1?.Entity, d2?.Entity));

        public bool Equals(EntityDecorator other) => !(other is null) && IsEqual(this, other);

        public bool Equals(IEntity other) => !(other is null) && Equals(Entity, other);

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EntityDecorator) obj);
        }

        public override int GetHashCode() => Entity?.GetHashCode() ?? 0;
    }
}
