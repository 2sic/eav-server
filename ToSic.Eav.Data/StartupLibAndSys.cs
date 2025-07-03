using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Startup;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupLibAndSys
{
    public static IServiceCollection AddEavCoreLibAndSys(this IServiceCollection services)
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
            .AddLibLookUpFallback()
            .AddSysSecurityFallback()
            .AddSysCapabilitiesFallback();

        return services;
    }
}