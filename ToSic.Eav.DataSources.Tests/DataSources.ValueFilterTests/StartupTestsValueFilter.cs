using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav.DataSources.ValueFilterTests;

internal class StartupTestsValueFilter: StartupCoreDataSourcesAndTestData
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddTransient<ValueFilterMaker>();
    }
}