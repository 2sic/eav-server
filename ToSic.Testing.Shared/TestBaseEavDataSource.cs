using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSources;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEavDataSource(EavTestConfig testConfig = default)
    : TestBaseEavCore(testConfig)
{
    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services
            // DataSources
            .AddDataSources();
    }

}