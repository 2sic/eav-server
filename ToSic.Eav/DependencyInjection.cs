using ToSic.Eav.DataSources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Run;
using ToSic.Eav.WebApi;

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
            // core things - usually not replaced
            services.TryAddTransient<IRuntime, Runtime>();
            
            // standard IEntity conversion
            // not sure where to put it, interface is in Core but the implementation in Web, also used by DataSources for json errors
            services.TryAddTransient<IConvertToEavLight, ConvertToEavLight>();

            // todo: wip moving DataSource stuff into that DLL
            services
                .AddEavApps()
                .AddFallbackAppServices()
                .AddImportExport()
                .AddRepositoryAndEfc()
                .AddDataSources()
                .AddEavWebApi()
                .AddEavCore()
                .AddEavCorePlumbing()
                .AddEavCoreFallbackServices();


            return services;
        }
    }
}
