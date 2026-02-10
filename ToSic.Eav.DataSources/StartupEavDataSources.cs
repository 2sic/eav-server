using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Sys;
using ToSic.Eav.Services;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupEavDataSources
{
    public static IServiceCollection AddDataSources(this IServiceCollection services)
    {
        // Dependencies, new in v15
        services.TryAddTransient<App.Dependencies>();

        services.TryAddTransient<IDataSourcesService, DataSourcesService>();
        services.TryAddTransient(typeof(IDataSourceGenerator<>), typeof(DataSourceGenerator<>));

        services.TryAddTransient<Sql>();
        services.TryAddTransient<Sql.Dependencies>();
        services.TryAddTransient<SqlPlatformInfo>();

        services.TryAddTransient<DataTable>();

        services.TryAddTransient<ValueLanguages>();

        services.TryAddTransient<ITreeMapper, TreeMapper>();

        // wip v21.02
        services.TryAddTransient<FeaturesForDataSources>();

        return services;
    }

    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static IServiceCollection AddDataSourcesFallbacks(this IServiceCollection services)
    //{
    //    //services.TryAddTransient<IAppDataSourcesLoader, AppDataSourcesLoaderUnknown>();

    //    return services;
    //}
}