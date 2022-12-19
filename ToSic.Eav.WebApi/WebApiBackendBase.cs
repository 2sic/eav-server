﻿using System;
using ToSic.Lib.DI;

namespace ToSic.Eav.WebApi
{
    public abstract class WebApiBackendBase<T>: ServiceWithLog where T : class
    {
        protected WebApiBackendBase(IServiceProvider serviceProvider, string logName) : base(logName)
        {
            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;

        public TService GetService<TService>() => _serviceProvider.Build<TService>();
    }
}
