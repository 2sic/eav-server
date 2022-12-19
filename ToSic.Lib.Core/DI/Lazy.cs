﻿using System;

namespace ToSic.Lib.DI
{
    /// <summary>
    /// Enables lazy requesting of objects - won't be available until needed
    /// </summary>
    /// <remarks>
    /// Inspired by https://www.programmersought.com/article/54291773421/
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class LazyDependencyInjection<T> : Lazy<T>
    {
        public LazyDependencyInjection(IServiceProvider sp) : base(sp.Build<T>)
        {
        }

    }
}
