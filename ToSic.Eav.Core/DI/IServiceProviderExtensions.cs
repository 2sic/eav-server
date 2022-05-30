using System;
using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav.DI
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceProviderExtensions
    {
        public static T Build<T>(this IServiceProvider serviceProvider)
        {
            var found = serviceProvider.GetService<T>();

            return found != null 
                ? found
                // If it's an unregistered type, try to find in DLLs etc.
                : ActivatorUtilities.CreateInstance<T>(serviceProvider);

        }


        public static T Build<T>(this IServiceProvider serviceProvider, Type type) where T: class
        {
            var found = serviceProvider.GetService(type);
            if (found != null) return found as T;

            // If it's an unregistered type, try to find in DLLs etc.
            return ActivatorUtilities.CreateInstance(serviceProvider, type) as T;
        }

    }
}
