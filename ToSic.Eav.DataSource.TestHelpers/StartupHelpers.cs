using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSource;
using ToSic.Eav.LookUp;
using ToSic.Eav.TestData;

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
            .AddTransient<DataSourcesTstBuilder>()
            .AddTransient<DataTableTrivial>()
            .AddTransient<DataTablePerson>()
            .AddTransient<LookUpTestData>()
        ;


}