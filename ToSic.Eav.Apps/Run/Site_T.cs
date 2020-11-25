using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    /// <summary>
    /// A tenant in the environment with a reference to the original thing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public abstract class Site<T> :ISite, IWrapper<T>
    {
        /// <inheritdoc />
        public abstract ISite Init(int siteId);

        /// <summary>
        /// The tenant settings - usually the DNN PortalSettings
        /// </summary>
        public virtual T UnwrappedContents { get; protected set; }


        /// <inheritdoc />
        public abstract string DefaultLanguage { get; }

        /// <inheritdoc />
        public abstract int Id { get; }

        /// <inheritdoc />
        public abstract string Name { get; }

        public abstract string Url { get; }

        /// <inheritdoc />
        [PrivateApi] public abstract string AppsRootPhysical { get; }

        [PrivateApi] public abstract string AppsRootPhysicalFull { get; }

        [PrivateApi] public virtual string AppsRootLink => AppsRootPhysical;

        [PrivateApi]
        public abstract bool RefactorUserIsAdmin { get; }

        [PrivateApi]
        public abstract string ContentPath { get; }


        public abstract int ZoneId { get; }
    }
}
