using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Run
{
    /// <summary>
    /// A tenant in the environment with a reference to the original thing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicApi]
    public abstract class Tenant<T> :ITenant, IWrapper<T>
    {
        /// <summary>
        /// The tenant settings - usually the DNN PortalSettings
        /// </summary>
        public T UnwrappedContents { get; }

        /// <inheritdoc />
        public abstract string DefaultLanguage { get; }

        /// <inheritdoc />
        public abstract int Id { get; }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        [PrivateApi]
        public abstract string SxcPath { get; }

        [PrivateApi]
        public abstract bool RefactorUserIsAdmin { get; }

        [PrivateApi]
        public abstract string ContentPath { get; }


        protected Tenant(T settings) => UnwrappedContents = settings;

    }
}
