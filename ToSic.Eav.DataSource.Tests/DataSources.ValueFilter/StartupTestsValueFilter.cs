using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.StartupTests;

namespace ToSic.Eav.DataSources.ValueFilter;

internal class StartupTestsValueFilter: StartupTestsEavCoreAndDataSources
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddTransient<ValueFilterMaker>();
    }
}