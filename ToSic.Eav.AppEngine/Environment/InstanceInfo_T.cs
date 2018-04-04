using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Environment
{
    public abstract class EnvironmentInstance<T>: IInstanceInfo
    {
        public T Original { get; }

        protected EnvironmentInstance(T item)
        {
            Original = item;
        }

        public abstract int Id { get; }

        public abstract int PageId { get; }

        public abstract int TenantId { get; }

        public abstract bool IsPrimary { get; }
    }
}
