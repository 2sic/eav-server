using System;
using ToSic.Eav.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.WebApi
{
    public abstract class WebApiBackendBase<T>: HasLog<T> where T : class
    {
        protected WebApiBackendBase(IServiceProvider serviceProvider, string logName) : base(logName)
        {
            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;

        public TService GetService<TService>() => _serviceProvider.Build<TService>();
    }
}
