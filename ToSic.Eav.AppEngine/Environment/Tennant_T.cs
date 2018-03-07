using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Environment
{
    public abstract class Tennant<T> :ITennant
    {
        /// <summary>
        /// The tennant settings - usually the DNN PortalSettings
        /// </summary>
        public T Settings { get; }

        /// <inheritdoc />
        public abstract string DefaultLanguage { get; }

        /// <inheritdoc />
        public abstract int Id { get; }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string SxcPath { get; }

        public abstract bool RefactorUserIsAdmin { get; }

        public abstract string ContentPath { get; }


        protected Tennant(T settings) => Settings = settings;
    }
}
