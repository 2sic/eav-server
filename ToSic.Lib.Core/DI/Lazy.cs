using System;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.DI
{
    /// <summary>
    /// Enables lazy requesting of objects - won't be available until needed
    /// </summary>
    /// <remarks>
    /// Inspired by https://www.programmersought.com/article/54291773421/
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    [PrivateApi]
    public class LazyDependencyInjection<T> : Lazy<T>
    {
        public LazyDependencyInjection(IServiceProvider sp) : base(sp.Build<T>)
        {
        }

    }
}
