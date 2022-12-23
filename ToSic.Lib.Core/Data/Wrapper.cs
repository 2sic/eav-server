using ToSic.Lib.Documentation;

namespace ToSic.Lib.Data
{
    /// <summary>
    /// Helper base class for all wrappers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PrivateApi("probably move to Lib soon")]
    public abstract class Wrapper<T>: IWrapper<T>
    {
        /// <summary>
        /// Property with the contents - should be used for all private/internal access to the UnwrappedContent
        /// </summary>
        [PrivateApi("should never appear in documentations")]
        protected virtual T UnwrappedContents { get; private set; }

        public T GetContents() => UnwrappedContents;

        protected Wrapper(T contents) => UnwrappedContents = contents;

        protected void Wrap(T contents) => UnwrappedContents = contents;
    }
}
