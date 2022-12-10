using System;

namespace ToSic.Eav.DI
{
    /// <summary>
    /// Enables generating additional objects of a specific type
    /// </summary>
    public class Generator<T> : IGenerator<T>
    {
        public Generator(IServiceProvider sp) => _sp = sp;
        private readonly IServiceProvider _sp;

        public T New => _sp.Build<T>();
    }
}
