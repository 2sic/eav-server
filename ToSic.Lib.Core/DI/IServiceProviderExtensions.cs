using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.Logging;

namespace ToSic.Lib.DI
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceProviderExtensions
    {
        /// <summary>
        /// Build a service from DI or if not found, try ActivatorUtilities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static T Build<T>(this IServiceProvider serviceProvider)
        {
            var found = serviceProvider.GetService<T>();

            return found != null 
                ? found
                // If it's an unregistered type, try to find in DLLs etc.
                : ActivatorUtilities.CreateInstance<T>(serviceProvider);
        }

        /// <summary>
        /// Build a service and if it supports logging, attach it to the parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <param name="parentLog"></param>
        /// <returns></returns>
        public static T Build<T>(this IServiceProvider serviceProvider, ILog parentLog)
        {
            var service = serviceProvider.Build<T>();
            if (service is IHasLog withLog && parentLog != null) withLog.LinkLog(parentLog);
            return service;
        }


        public static T Build<T>(this IServiceProvider serviceProvider, Type type, ILog parentLog = null) where T: class
        {
            var service = serviceProvider.GetService(type);
            if (service is IHasLog withLog && parentLog != null) withLog.LinkLog(parentLog);
            if (service != null) return service as T;

            // If it's an unregistered type, try to find in DLLs etc.
            return ActivatorUtilities.CreateInstance(serviceProvider, type) as T;
        }

    }
}
