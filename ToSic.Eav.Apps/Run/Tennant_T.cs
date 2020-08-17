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

        public abstract string Url { get; }

        /// <inheritdoc />
        [PrivateApi]
        public abstract string AppsRoot { get; }

        [PrivateApi]
        public abstract bool RefactorUserIsAdmin { get; }

        [PrivateApi]
        public abstract string ContentPath { get; }


        protected Tenant(T settings) => UnwrappedContents = settings;

    }
}
