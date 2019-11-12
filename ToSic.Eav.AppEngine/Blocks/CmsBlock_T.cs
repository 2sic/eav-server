namespace ToSic.Eav.Apps.Blocks
{
    public abstract class CmsBlock<T>: ICmsBlock
    {
        public T Original { get; }

        protected CmsBlock(T item)
        {
            Original = item;
        }

        public abstract int Id { get; }

        public abstract int PageId { get; }

        public abstract int TenantId { get; }

        public abstract bool IsPrimary { get; }
    }
}
