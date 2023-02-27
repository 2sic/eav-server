using System;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Data
{
    /// <summary>
    /// Helper base class for all wrappers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PrivateApi]
    public abstract class Wrapper<T>: IWrapper<T>
    {
        ///// <summary>
        ///// Property with the contents - should be used for all private/internal access to the UnwrappedContent
        ///// </summary>
        //[PrivateApi("should never appear in documentations")]
        //protected virtual T UnwrappedContents => _unwrappedContents;

        private T _unwrappedContents;

        /// <inheritdoc />
        public virtual T GetContents() => _unwrappedContents;

        protected Wrapper(T contents) => _unwrappedContents = contents;


        protected void Wrap(T contents) => _unwrappedContents = contents;
    }
}
