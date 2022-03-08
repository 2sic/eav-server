using System;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.WebApi
{
    public abstract class WebApiBackendBase<T>: HasLog<T> where T : class
    {
        protected WebApiBackendBase(IServiceProvider serviceProvider, string logName) : base(logName)
        {
            ServiceProvider = serviceProvider;
        }
        protected IServiceProvider ServiceProvider { get; }

        public TService GetService<TService>() => ServiceProvider.Build<TService>();
    }
}
