using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Metadata;
using ToSic.Lib;
using ToSic.Lib.Boot;
using ToSic.Lib.Caching;
using ToSic.Sys;

namespace ToSic.Eav.StartUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartUpEavCore
{
    public static IServiceCollection AddEavCoreLibAndSys(this IServiceCollection services)
    {
        services
            .AddEavCore()
            .AddSysCapabilities()
            .AddSysSecurity()
            .AddSysCode()
            .AddSysUtils()
            .AddLibCore();

        return services;
    }

    public static IServiceCollection AddEavCore(this IServiceCollection services)
    {
        // v17
        services.TryAddTransient<MemoryCacheService>();

        // v20 Startup
        services.AddTransient<IBootProcess, BootRegistrationEavFeatures>();
        services.AddTransient<IBootProcess, BootRegistrationEavLicenses>();
        services.TryAddTransient<ITargetTypeService, TargetTypesService>();

        return services;
    }

}