using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSources.ValueFilter;

internal class StartupTestsValueFilter: StartupCoreDataSourcesAndTestData
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddTransient<ValueFilterMaker>();
    }
}