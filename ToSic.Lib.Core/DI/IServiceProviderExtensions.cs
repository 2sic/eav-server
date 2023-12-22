using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.Logging;

namespace ToSic.Lib.DI;

/// <summary>
/// Extension methods used for dependency injection work.
/// Mostly internal.
/// </summary>
// ReSharper disable once InconsistentNaming
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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
        // Try to first check registered types, otherwise try to find in DLLs etc. using Activator Utilities
        var found = serviceProvider.GetService<T>();
        // If it's an unregistered type, try to find in DLLs etc. - note that ?? doesn't work
        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        if (found == null) found = ActivatorUtilities.CreateInstance<T>(serviceProvider);
        return found;
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
        // Try to first check registered types, otherwise try to find in DLLs etc. using Activator Utilities
        var service = serviceProvider.GetService(type)
                      ?? ActivatorUtilities.CreateInstance(serviceProvider, type);
        if (service is IHasLog withLog && parentLog != null) withLog.LinkLog(parentLog);
        return service as T;
    }

}