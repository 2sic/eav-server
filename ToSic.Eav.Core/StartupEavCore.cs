﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Eav.Types;

namespace ToSic.Eav
{
    public static class StartupEavCore
    {
        public static IServiceCollection AddEavCore(this IServiceCollection services)
        {
            // 2020-10-29 New enhancement - lazy loading dependency injection
            services.AddTransient(typeof(Lazy<>), typeof(LazyDependencyInjection<>));

            // Data Builder & Converters
            services.TryAddTransient<IDataBuilder, DataBuilder>();
            
            // Global Content-Types - should only be loaded once ever, and then it's done
            services.TryAddSingleton<GlobalTypeLoader>();

            // Configuration objects
            services.TryAddTransient<IGlobalConfiguration, GlobalConfiguration>();
            services.TryAddTransient<IDbConfiguration, DbConfiguration>();
            
            // try to drop this / replace with...
            services.TryAddTransient<IFeaturesConfiguration, Features>();
            // ...with this
            services.TryAddTransient<SystemLoader>();
            services.TryAddTransient<Features>();

            // App-State and Cache
            services.TryAddSingleton<IAppsCache, AppsCache>();
            services.TryAddTransient<IAppStates, AppStates>();
            services.TryAddTransient<AppsCacheBase.Dependencies>();

            // Other...
            services.TryAddTransient<AttributeBuilder>();

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
            services.TryAddTransient<IFingerprint, FingerprintUnknown>();
            services.TryAddTransient<IValueConverter, ValueConverterBase>();

            services.TryAddTransient<ILookUpEngineResolver, LookUpEngineResolverUnknown>();
            services.TryAddTransient<IUser, UserUnknown>();
            services.TryAddTransient<IZoneCultureResolver, ZoneCultureResolverUnknown>();
            services.TryAddTransient<IServerPaths, ServerPathsUnknown>();
            services.TryAddTransient<IAppRepositoryLoader, AppRepositoryLoaderUnknown>();
            return services;
        }

    }
}
