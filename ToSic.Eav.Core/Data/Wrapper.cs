using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Helper base class for all wrappers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Wrapper<T>: IWrapper<T>
    {
        /// <summary>
        /// Property with the contents - should be used for all internal access to the UnwrappedContent
        /// </summary>
        [PrivateApi]
        protected T _contents;

        [PrivateApi]
        public T UnwrappedContents => _contents;

        public T GetContents() => _contents;

        protected Wrapper(T contents) => _contents = contents;

        protected void Init(T contents) => _contents = contents;
    }
}
