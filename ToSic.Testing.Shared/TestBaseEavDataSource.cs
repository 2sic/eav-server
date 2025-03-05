using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSources;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEavDataSource(EavTestConfig testConfig = default)
    : TestBaseEavCore(testConfig)
{
    protected override IServiceCollection SetupServices(IServiceCollection services) =>
        base.SetupServices(services)
            // DataSources
            .AddDataSources();
}