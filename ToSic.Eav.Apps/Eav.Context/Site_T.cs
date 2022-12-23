using ToSic.Eav.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    /// <summary>
    /// A tenant in the environment with a reference to the original thing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PrivateApi]
    public abstract class Site<T>: HasLog, ISite, IWrapper<T>
    {
        protected Site(string logPrefix): base($"{logPrefix}.Site") { }

        /// <inheritdoc />
        public abstract ISite Init(int siteId, ILog parentLog);

        public virtual T GetContents() => UnwrappedSite;
        [PrivateApi] protected virtual T UnwrappedSite { get; set; }

        /// <inheritdoc />
        public abstract string CurrentCultureCode { get; }

        /// <inheritdoc />
        public abstract string DefaultCultureCode { get; }

        /// <inheritdoc />
        public abstract int Id { get; }

        /// <inheritdoc />
        public abstract string Name { get; }

        public abstract string Url { get; }

        public abstract string UrlRoot { get; }

        /// <inheritdoc />
        [PrivateApi] public abstract string AppsRootPhysical { get; }

        [PrivateApi] public abstract string AppsRootPhysicalFull { get; }

        [PrivateApi] public abstract string AppAssetsLinkTemplate { get; }

        [PrivateApi]
        public abstract string ContentPath { get; }


        public abstract int ZoneId { get; }
    }
}
