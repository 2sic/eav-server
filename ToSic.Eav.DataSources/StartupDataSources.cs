using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Internal;
using ToSic.Eav.DataSources.Sys;
using ToSic.Eav.Services;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Startup;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupDataSources
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddDataSources(this IServiceCollection services)
    {
        // Dependencies, new in v15
        services.TryAddTransient<App.MyServices>();

        services.TryAddTransient<IDataSourcesService, DataSourcesService>();
        services.TryAddTransient(typeof(IDataSourceGenerator<>), typeof(DataSourceGenerator<>));

        services.TryAddTransient<Sql>();
        services.TryAddTransient<Sql.MyServices>();
        services.TryAddTransient<SqlPlatformInfo>();

        services.TryAddTransient<DataTable>();

        services.TryAddTransient<ValueLanguages>();

        services.TryAddTransient<ITreeMapper, TreeMapper>();

        return services;
    }

    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static IServiceCollection AddDataSourcesFallback(this IServiceCollection services)
    //{
    //    //services.TryAddTransient<IAppDataSourcesLoader, AppDataSourcesLoaderUnknown>();

    //    return services;
    //}
}