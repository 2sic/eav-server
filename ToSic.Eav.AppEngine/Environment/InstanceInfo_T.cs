using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Environment
{
    public abstract class InstanceInfo<T>: IInstanceInfo
    {
        public T Info { get; }

        protected InstanceInfo(T item)
        {
            Info = item;
        }

        public abstract int Id { get; }

        public abstract int PageId { get; }

        public abstract int TennantId { get; }

        public abstract bool IsPrimary { get; }
    }
}
