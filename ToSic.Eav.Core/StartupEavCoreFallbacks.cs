using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Global.Sys;
using ToSic.Lib.LookUp;
using ToSic.Sys;

namespace ToSic.Eav.StartUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartUpEavCoreFallbacks
{
    /// <summary>
    /// This will add Do-Nothing services of EAV Core, Lib and Sys.
    /// </summary>
    /// <remarks>
    /// All calls in here MUST use TryAddTransient, and never without the Try
    /// </remarks>
    public static IServiceCollection AddEavCoreLibAndSysFallbackServices(this IServiceCollection services)
    {
        services
            .AddEavCoreOnlyFallbackServices()
            .AddLibLookUpFallback()
            .AddSysSecurityFallback()
            .AddSysCapabilitiesFallback();

        return services;
    }

    /// <summary>
    /// This will add Do-Nothing services which will take over if they are not provided by the main system
    /// In general this will result in some features missing, which many platforms don't need or care about
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <remarks>
    /// All calls in here MUST use TryAddTransient, and never without the Try
    /// </remarks>
    public static IServiceCollection AddEavCoreOnlyFallbackServices(this IServiceCollection services)
    {
        // At the moment there is nothing to add here
        services.TryAddTransient<IGlobalContentTypesService, GlobalContentTypesServiceUnknown>();

        return services;
    }
}