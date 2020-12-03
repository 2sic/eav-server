﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Data.ValueConverter;
using ToSic.Eav.LookUp;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Basic;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav
{
    public static class StartupEavCore
    {
        public static IServiceCollection AddEavCore(this IServiceCollection services)
        {
            // 2020-10-29 New enhancement - lazy loading dependency injection
            services.AddTransient(typeof(Lazy<>), typeof(LazyDependencyInjection<>));

            // Configuration objects
            services.TryAddTransient<IDbConfiguration, DbConfiguration>();
            services.TryAddTransient<IFeaturesConfiguration, FeaturesConfiguration>();

            // App-State and Cache
            services.TryAddSingleton<IAppsCache, AppsCache>();

            // Metadata providers
            services.TryAddTransient<IRemoteMetadata, RemoteMetadata>();



            // todo
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
            services.TryAddTransient<IFingerprint, UnknownFingerprint>();
            services.TryAddTransient<IValueConverter, ValueConverterBase>();

            services.TryAddTransient<ILookUpEngineResolver, UnknownLookUpEngineResolver>();
            services.TryAddTransient<IUser, UnknownUser>();
            services.TryAddTransient<IGetDefaultLanguage, UnknownGetDefaultLanguage>();
            services.TryAddTransient<IServerPaths, UnknownServerPaths>();
            return services;
        }

    }
}
