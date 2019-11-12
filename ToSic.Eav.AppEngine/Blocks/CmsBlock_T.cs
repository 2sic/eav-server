﻿using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Blocks
{
    /// <summary>
    /// A base implementation of the block information wrapping the CMS specific object along with it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicApi]
    public abstract class CmsBlock<T>: ICmsBlock<T>
    {
        /// <inheritdoc />
        public T Original { get; }


        protected CmsBlock(T item)
        {
            Original = item;
        }

        /// <inheritdoc cref="ICmsBlock" />
        public abstract int Id { get; }

        /// <inheritdoc cref="ICmsBlock" />
        public abstract int PageId { get; }

        /// <inheritdoc cref="ICmsBlock" />
        public abstract int TenantId { get; }

        /// <inheritdoc cref="ICmsBlock" />
        public abstract bool IsPrimary { get; }
    }
}
