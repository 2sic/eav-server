using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Internal.Compression;
using ToSic.Eav.Internal.Environment;
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
        // Configuration objects
        services.TryAddTransient<GlobalPaths>();

        // Features - 2024-05-31 changed to non-singleton
        services.TryAddTransient<ISysFeaturesService, SysFeaturesService>();    // this must come first!
        services.TryAddTransient<ILibFeaturesService, SysFeaturesService>();    // v20

        // App-State and Cache
        //services.AddTransient<AppsCacheSwitch>();   // Transient should work... wip
        //services.TryAddTransient<IAppStateCacheService, AppStateCacheService>();
        //services.TryAddTransient<IAppsCatalog, AppsCatalog>(); // new v18
        //services.TryAddTransient<IAppReaderFactory, AppReaderFactory>(); // new v18
        services.TryAddTransient<IAppStateBuilder, AppState.AppStateBuilder>();
        //services.TryAddTransient<AppReader>();
        //services.TryAddTransient<AppDataStackService>();

        // Permissions helper
        services.TryAddTransient<PermissionCheckBase.MyServices>();

        //services.TryAddTransient<LicenseLoader>();

        services.TryAddTransient<Compressor>();

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