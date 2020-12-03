using System;
using ToSic.Eav.DataSources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Metadata;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Basic;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public static class StartupEav
	{
	    /// <summary>
	    /// Use this to setup the new DI container
	    /// </summary>
	    /// <param name="services"></param>
	    public static IServiceCollection AddEav(this IServiceCollection services)
	    {
            // 2020-10-29 New enhancement - lazy loading dependency injection
            services.AddTransient(typeof(Lazy<>), typeof(LazyDependencyInjection<>));

            // very basic stuff - normally overriden by the platform
            services.TryAddTransient<IFingerprint, BasicFingerprint>();
            services.TryAddTransient<IUser, BasicUser>();
            services.TryAddTransient<IValueConverter, BasicValueConverter>();

            // core things - usually not replaced
            services.TryAddTransient<IRuntime, Runtime>();

            services.TryAddSingleton<IAppsCache, AppsCache>();

	        services.TryAddTransient<IRemoteMetadata, RemoteMetadata>();

            // todo: wip moving DataSource stuff into that DLL
            services
                .AddEavApps()
                .AddFallbackAppServices()
                .AddImportExport()
                .AddRepositoryAndEfc()
                .AddDataSources()
                .AddEavCore()
                .AddEavCoreFallbackServices();


            return services;
        }
    }
}
