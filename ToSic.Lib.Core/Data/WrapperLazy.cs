using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Lib.Data
{
    public class WrapperLazy<T>: Wrapper<T> where T : class
    {
        public WrapperLazy(T contents) : base(contents)
        {
        }

        /// <summary>
        /// Overload for lazy wrapper.
        /// </summary>
        /// <param name="getContents"></param>
        protected WrapperLazy(Func<T> getContents): base(default) => _getContents = getContents;

        private T _unwrappedContents;

        protected void Reset() => Wrap(default);

        private readonly Func<T> _getContents;

        /// <inheritdoc />
        public override T GetContents()
        {
            if (_unwrappedContents != default) return _unwrappedContents;
            _unwrappedContents = base.GetContents() ?? _getContents();
            return _unwrappedContents;
        }
    }
}
