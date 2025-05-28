using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security.Encryption;
using ToSic.Lib.Caching;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Code.InfoSystem;
using ToSic.Sys.Security.Permissions;
using CodeInfoService = ToSic.Sys.Code.InfoSystem.CodeInfoService;

namespace ToSic.Eav.StartUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartUpEavCore
{
    public static IServiceCollection AddEavCore(this IServiceCollection services)
    {
        // Features - 2024-05-31 changed to non-singleton
        services.TryAddTransient<ISysFeaturesService, SysFeaturesService>();    // this must come first!
        services.TryAddTransient<ILibFeaturesService, SysFeaturesService>();    // v20

        // Permissions helper
        services.TryAddTransient<PermissionCheckBase.MyServices>();

        services.TryAddTransient<AesCryptographyService>();
        services.TryAddTransient<Rfc2898Generator>();

        // V16.02 - Obsolete service
        services.TryAddTransient<CodeInfoService>();
        services.TryAddScoped<CodeInfosInScope>();
        services.TryAddTransient<CodeInfoStats>();

        // v17
        services.TryAddTransient<MemoryCacheService>();

        // v18
        services.TryAddTransient<RsaCryptographyService>();
        services.TryAddTransient<AesHybridCryptographyService>();

        // v20 Startup
        services.AddTransient<IStartUpRegistrations, EavStartupRegistrations>();
        services.TryAddTransient<ITargetTypeService, TargetTypesService>();

        return services;
    }

}