using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Catalog;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Internal.Compression;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Encryption;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Lib.Code.InfoSystem;
using CodeInfoService = ToSic.Lib.Code.InfoSystem.CodeInfoService;

namespace ToSic.Eav.StartUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartUpEavCore
{
    public static IServiceCollection AddEavCore(this IServiceCollection services)
    {
        // Configuration objects
        services.TryAddTransient<IDbConfiguration, DbConfiguration>();
        services.TryAddTransient<GlobalPaths>();
        services.TryAddTransient<SystemLoader>();
        services.TryAddTransient<EavSystemLoader>();
        services.TryAddTransient<EavFeaturesLoader>();  // new v20 separate class
        services.TryAddTransient<FeaturePersistenceService>();
        services.TryAddTransient<FeaturesIoHelper>();

        // Features - 2024-05-31 changed to non-singleton
        services.TryAddTransient<IEavFeaturesService, EavFeaturesService>();    // this must come first!
        services.TryAddTransient<ILibFeaturesService, EavFeaturesService>();    // v20

        // App-State and Cache
        services.TryAddSingleton<IAppsCache, AppsCache>();
        services.TryAddTransient<IAppLoaderTools, AppLoaderTools>();
        services.TryAddTransient<IAppStateCacheService, AppStateCacheService>();
        services.TryAddTransient<IAppsCatalog, AppsCatalog>(); // new v18
        services.TryAddTransient<IAppReaderFactory, AppReaderFactory>(); // new v18
        services.TryAddTransient<IAppStateBuilder, AppState.AppStateBuilder>();
        services.TryAddTransient<AppReader>();
        services.TryAddTransient<AppDataStackService>();
        services.TryAddTransient<AppLoaderLogSettings>();   // new v20

        // v13 #SwitchableAppsCache
        services.AddSingleton<IAppsCacheSwitchable, AppsCache>();
        services.AddTransient<AppsCacheSwitch>();   // Transient should work... wip
            
        // Permissions helper
        services.TryAddTransient<PermissionCheckBase.MyServices>();

        // Fingerprinting: Because fo security, we are not injecting the interface
        // As that would allow replacing the finger-printer with something else
        // We actually only use the direct object in DI
        //services.TryAddTransient<IFingerprint, Fingerprint>();
        services.TryAddTransient<SystemFingerprint>();

        services.TryAddTransient<LicenseLoader>();

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

        return services;
    }

}