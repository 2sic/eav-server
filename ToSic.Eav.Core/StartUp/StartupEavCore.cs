﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Compression;
using ToSic.Eav.Configuration;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Persistence;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Lib.DI;

namespace ToSic.Eav.StartUp
{
    public static class StartUpEavCore
    {
        public static IServiceCollection AddEavCore(this IServiceCollection services)
        {
            // Data Builder & Converters
            services.TryAddTransient<IDataBuilder, DataBuilder>();
            services.TryAddTransient<MultiBuilder>();
            services.TryAddTransient<DimensionBuilder>();
            services.TryAddTransient<AttributeBuilder>();
            services.TryAddTransient<AttributeBuilderForImport>();
            services.TryAddTransient<EntityBuilder>();
            services.TryAddTransient<EntitySaver>();
            services.TryAddTransient<ValueBuilder>();
            services.TryAddTransient<ContentTypeBuilder>();
            services.TryAddTransient<ContentTypeAttributeBuilder>();

            // Configuration objects
            services.TryAddTransient<IGlobalConfiguration, GlobalConfiguration>();
            services.TryAddTransient<IDbConfiguration, DbConfiguration>();
            services.TryAddTransient<GlobalPaths>();
            services.TryAddTransient<SystemLoader>();
            services.TryAddTransient<EavSystemLoader>();
            services.TryAddTransient<FeatureConfigManager>();

            // Make sure that IFeaturesInternal and IFeatures use the same singleton!
            services.AddSingleton<LicenseCatalog>();    // Must be singleton
            services.AddSingleton<FeaturesCatalog>();   // Must be singleton
            services.TryAddSingleton<IFeaturesInternal, FeaturesService>();    // this must come first!
            services.TryAddSingleton<IFeaturesService>(x => x.GetRequiredService<IFeaturesInternal>());

            // App-State and Cache
            services.TryAddSingleton<IAppsCache, AppsCache>();
            services.TryAddTransient<IAppLoaderTools, AppLoaderTools>();
            services.TryAddTransient<IAppStates, AppStates>();
            services.TryAddTransient<AppSettingsStack>();

            // v13 #SwitchableAppsCache
            services.AddSingleton<IAppsCacheSwitchable, AppsCache>();
            services.AddTransient<AppsCacheSwitch>();   // Transient should work... wip


            // Permissions helper
            services.TryAddTransient<PermissionCheckBase.Dependencies>();

            services.TryAddTransient<ILicenseService, LicenseService>();

            // Fingerprinting: Because fo security, we are not injecting the interface
            // As that would allow replacing the fingerprinter with something else
            // We actually only use the direct object in DI
            //services.TryAddTransient<IFingerprint, Fingerprint>();
            services.TryAddTransient<SystemFingerprint>();

            services.TryAddSingleton<LightSpeedStats>();

            // V14 Requirements Checks - don't use try-add, as we'll add many
            services.TryAddTransient<RequirementsService>();
            services.AddTransient<IRequirementCheck, RequirementCheckFeature>();

            services.TryAddTransient<Compressor>();

            return services;
        }

        /// <summary>
        /// Very basic internal services which are necessary for even the most basic DI to work
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEavCorePlumbing(this IServiceCollection services)
        {
            // Lazy loading dependency injection
            services.AddTransient(typeof(Lazy<>), typeof(LazyDependencyInjection<>));
            services.AddTransient(typeof(LazyInit<>));
            services.AddTransient(typeof(LazyInitLog<>));
            services.AddTransient(typeof(Generator<>));
            services.AddTransient(typeof(IGenerator<>), typeof(Generator<>));
            services.AddTransient(typeof(GeneratorLog<>));

            // Service Switchers
            services.TryAddTransient(typeof(ServiceSwitcher<>));
            services.TryAddScoped(typeof(ServiceSwitcherScoped<>)); // note: it's for scoped, and we must use another object name here
            services.TryAddTransient(typeof(ServiceSwitcherSingleton<>)); // note: it's for singletons, but the service is transient on purpose!

            // History (very core service)
            services.TryAddTransient<History>();

            // Warnings for mock implementations
            services.TryAddTransient(typeof(WarnUseOfUnknown<>));
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
        public static IServiceCollection AddEavCoreFallbackServices(this IServiceCollection services)
        {
            // very basic stuff - normally overriden by the platform
            services.TryAddTransient<IValueConverter, ValueConverterUnknown>();

            services.TryAddTransient<ILookUpEngineResolver, LookUpEngineResolverUnknown>();
            services.TryAddTransient<IUser, UserUnknown>();
            services.TryAddTransient<IZoneCultureResolver, ZoneCultureResolverUnknown>();
            services.TryAddTransient<IServerPaths, ServerPathsUnknown>();
            services.TryAddTransient<IAppRepositoryLoader, AppRepositoryLoaderUnknown>();

            // Special registration of iisUnknown to verify we see warnings if such a thing is loaded
            services.TryAddTransient<IIsUnknown, ServerPathsUnknown>();

            // Unknown-Runtime for loading configuration etc. File-runtime
            services.TryAddTransient<IRuntime, RuntimeUnknown>();
            services.TryAddTransient<IPlatformInfo, PlatformUnknown>();

            return services;
        }

    }
}
