using System;
using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav.Plumbing
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceProviderExtensions
    {
        public static T Build<T>(this IServiceProvider serviceProvider) where T: class
        {
            var found = serviceProvider.GetService<T>();
            // If it's an unregistered type, try to find in DLLs etc.
            return found ?? ActivatorUtilities.CreateInstance<T>(serviceProvider);
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
