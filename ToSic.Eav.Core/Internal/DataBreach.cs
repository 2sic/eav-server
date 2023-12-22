using ToSic.Eav.Data;
using ToSic.Lib.Data;

namespace ToSic.Eav.Internal
{
    public static class DataBreach
    {
        public static int? GetPublishedEntityId(this IEntity entity) => entity.GetEntity()?.PublishedEntityId;

        private static Entity GetEntity(this IEntity entity)
        {
            if (entity is null) return null;
            if (entity is Entity e) return e;

            if (entity is not IWrapper<IEntity> wrapped) return null;

            entity = wrapped.GetContents();
            if (entity is Entity e2) return e2;

            if (entity is IWrapper<IEntity> rewrapped && rewrapped.GetContents() is Entity e3) return e3;

            return null;
        }
    }
}
