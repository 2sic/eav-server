using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Testing;

namespace ToSic.Eav.DataSources.ValueFilterTests;

internal class StartupTestsValueFilter: StartupCoreDataSourcesAndTestData
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services
            .AddDataSourceTestHelpers()
            .AddTransient<ValueFilterMaker>();
    }
}