﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Code.InfoSystem;
using ToSic.Eav.Compression;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.LookUp;
using ToSic.Eav.Persistence;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Capabilities;
using ToSic.Eav.Run.Requirements;
using ToSic.Eav.Run.Unknown;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Encryption;
using ToSic.Eav.Security.Fingerprint;
using CodeInfoService = ToSic.Eav.Code.InfoSystem.CodeInfoService;

namespace ToSic.Eav.StartUp
{
    public static class StartUpEavCore
    {
        public static IServiceCollection AddEavCore(this IServiceCollection services)
        {
            // Data Builder & Converters
            services.TryAddTransient<IDataFactory, DataFactory>(); // v15.03
            services.TryAddTransient<DataBuilder>();
            services.TryAddTransient<DimensionBuilder>();
            services.TryAddTransient<AttributeBuilder>();
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
            services.TryAddTransient<FeaturePersistenceService>();

            // Make sure that IFeaturesInternal and IFeatures use the same singleton!
            services.AddSingleton<LicenseCatalog>();    // Must be singleton
            services.AddSingleton<FeaturesCatalog>();   // Must be singleton
            services.TryAddSingleton<IEavFeaturesService, EavFeaturesService>();    // this must come first!

            // New SystemCapability
            services.TryAddTransient<SysFeaturesService>();

            // App-State and Cache
            services.TryAddSingleton<IAppsCache, AppsCache>();
            services.TryAddTransient<IAppLoaderTools, AppLoaderTools>();
            services.TryAddTransient<IAppStates, AppStates>();
            services.TryAddTransient<AppSettingsStack>();

            // v13 #SwitchableAppsCache
            services.AddSingleton<IAppsCacheSwitchable, AppsCache>();
            services.AddTransient<AppsCacheSwitch>();   // Transient should work... wip
            
            // Permissions helper
            services.TryAddTransient<PermissionCheckBase.MyServices>();

            services.TryAddTransient<ILicenseService, LicenseService>();

            // Fingerprinting: Because fo security, we are not injecting the interface
            // As that would allow replacing the fingerprinter with something else
            // We actually only use the direct object in DI
            //services.TryAddTransient<IFingerprint, Fingerprint>();
            services.TryAddTransient<SystemFingerprint>();

            services.TryAddSingleton<LightSpeedStats>();

            // V14 Requirements Checks - don't use try-add, as we'll add many
            services.TryAddTransient<RequirementsService>();
            services.AddTransient<IRequirementCheck, FeatureRequirementCheck>();

            services.TryAddTransient<LicenseLoader>();

            services.TryAddTransient<Compressor>();

            services.TryAddTransient<AesCryptographyService>();
            services.TryAddTransient<Rfc2898Generator>();

            // V16.02 - Obsolete service
            services.TryAddTransient<CodeInfoService>();
            services.TryAddScoped<CodeInfosInScope>();
            services.TryAddTransient<CodeInfoStats>();

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
            // Warnings for mock implementations
            services.TryAddTransient(typeof(WarnUseOfUnknown<>));

            // very basic stuff - normally overriden by the platform
            services.TryAddTransient<IValueConverter, ValueConverterUnknown>();

            services.TryAddTransient<ILookUpEngineResolver, LookUpEngineResolverUnknown>();
            services.TryAddTransient<IUser, UserUnknown>();
            services.TryAddTransient<IContextOfUserPermissions, ContextOfUserPermissions>();
            services.TryAddTransient<IContextResolverUserPermissions, ContextResolverUserPermissions>();
            services.TryAddTransient<IZoneCultureResolver, ZoneCultureResolverUnknown>();
            services.TryAddTransient<IServerPaths, ServerPathsUnknown>();
            services.TryAddTransient<IAppRepositoryLoader, AppRepositoryLoaderUnknown>();

            // Special registration of iisUnknown to verify we see warnings if such a thing is loaded
            services.TryAddTransient<IIsUnknown, ServerPathsUnknown>();

            // Unknown-Runtime for loading configuration etc. File-runtime
            services.TryAddTransient<IRuntime, RuntimeUnknown>();
            services.TryAddTransient<IPlatformInfo, PlatformUnknown>();

            services.TryAddTransient<IRequirementsService, RequirementsServiceUnknown>();

            return services;
        }

    }
}
