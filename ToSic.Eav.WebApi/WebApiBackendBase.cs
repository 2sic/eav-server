//using System;
//using ToSic.Lib.DI;
//using ToSic.Lib.Services;

//namespace ToSic.Eav.WebApi
//{
//    public abstract class WebApiBackendBase: ServiceBase
//    {
//        protected WebApiBackendBase(IServiceProvider serviceProvider, string logName) : base(logName)
//        {
//            _serviceProvider = serviceProvider;
//        }

//        private readonly IServiceProvider _serviceProvider;

//        public TService GetService<TService>() => _serviceProvider.Build<TService>();
//    }
//}
