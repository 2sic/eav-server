using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Environment
{
    public abstract class Tennant<T> :ITennant
    {
        public T Settings { get; }

        public abstract string DefaultLanguage { get; }

        public abstract int Id { get; }

        public abstract string RootPath { get; }

        protected Tennant(T settings)
        {
            Settings = settings;
        }
    }
}
