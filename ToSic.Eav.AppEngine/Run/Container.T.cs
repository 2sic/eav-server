using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Run;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Environment
{
    /// <summary>
    /// A base implementation of the block information wrapping the CMS specific object along with it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicApi]
    public abstract class Container<T>: IContainer, IWrapper<T>
    {
        /// <inheritdoc />
        public T Original { get; }


        protected Container(T item)
        {
            Original = item;
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
