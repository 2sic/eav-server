using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Run
{
    /// <summary>
    /// A base implementation of the block information wrapping the CMS specific object along with it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicApi]
    public abstract class Container<T>: IContainer, IWrapper<T>
    {
        /// <inheritdoc />
        public T UnwrappedContents { get; }


        protected Container(T item)
        {
            UnwrappedContents = item;
        }

        /// <inheritdoc />
        public abstract int Id { get; }

        /// <inheritdoc />
        public abstract int PageId { get; }

        /// <inheritdoc />
        public abstract int TenantId { get; }

        /// <inheritdoc />
        public abstract bool IsPrimary { get; }
    }
}
