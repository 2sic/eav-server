using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    /// <summary>
    /// A tenant in the environment with a reference to the original thing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PrivateApi]
    public abstract class Site<T>: ISite, IWrapper<T>
    {
        /// <inheritdoc />
        public abstract ISite Init(int siteId);

        /// <summary>
        /// The tenant settings - usually the DNN PortalSettings
        /// </summary>
        public virtual T UnwrappedContents { get; protected set; }
        public T GetContents() => UnwrappedContents;

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
