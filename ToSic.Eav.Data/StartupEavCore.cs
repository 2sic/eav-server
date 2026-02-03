using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupEavCore
{
    public static IServiceCollection AddAllLibAndSys(this IServiceCollection services)
    {
        services
            .AddSysCapabilities()
            .AddSysSecurity()
            .AddSysCode()
            .AddSysCaching()
            .AddSysUtils()
            .AddSysCore();

        return services;
    }

    /// <summary>
    /// This will add Do-Nothing services of EAV Core, Lib and Sys.
    /// </summary>
    /// <remarks>
    /// All calls in here MUST use TryAddTransient, and never without the Try
    /// </remarks>
    public static IServiceCollection AddAllLibAndSysFallbacks(this IServiceCollection services)
    {
        services
            .AddLibLookUpFallbacks()
            .AddSysSecurityFallbacks()
            .AddSysCapabilitiesFallbacks();

        return services;
    }
}