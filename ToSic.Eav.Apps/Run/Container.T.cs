using System.Collections.Generic;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    /// <summary>
    /// A base implementation of the block information wrapping the CMS specific object along with it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public abstract class Container<T>: IContainer, IWrapper<T> where T: class
    {
        #region Constructors and DI

        /// <inheritdoc />
        public T UnwrappedContents { get; private set; }


        protected Container(T item = null) => Init(item);

        public IContainer Init(T item)
        {
            UnwrappedContents = item;
            return this;
        }

        public abstract IContainer Init(int id, ILog parentLog);
        #endregion

        /// <inheritdoc />
        public abstract int Id { get; }

        /// <inheritdoc />
        public abstract bool IsPrimary { get; }

        /// <inheritdoc />
        public abstract IBlockIdentifier BlockIdentifier { get; }
    }
}
