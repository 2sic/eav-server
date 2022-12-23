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
        /// Property with the contents - should be used for all private/internal access to the UnwrappedContent
        /// </summary>
        [PrivateApi]
        protected virtual T UnwrappedContents { get; private set; }

        public T GetContents() => UnwrappedContents;

        protected Wrapper(T contents) => UnwrappedContents = contents;

        protected void Init(T contents) => UnwrappedContents = contents;
    }
}
