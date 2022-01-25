using System;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.WebApi
{
    public abstract class WebApiBackendBase<T>: HasLog<T> where T : class
    {
        public IServiceProvider ServiceProvider { get; }
        protected WebApiBackendBase(IServiceProvider serviceProvider, string logName) : base(logName)
        {
            ServiceProvider = serviceProvider;
        }

        public TService GetService<TService>() => ServiceProvider.Build<TService>();
    }
}
