using Microsoft.Extensions.DependencyInjection;
using ToSic.Testing.Shared;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Testing;

public static class StartupHelpers
{
    /// <summary>
    /// Add things necessary for DB fixtures to work.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDataSourceTestHelpers(this IServiceCollection services) =>
        services
            .AddTransient<DataSourcesTstBuilder>();
}