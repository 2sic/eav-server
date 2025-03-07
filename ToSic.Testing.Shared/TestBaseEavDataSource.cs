using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSources;
using ToSic.Eav.Testing;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEavDataSource(EavTestConfig testConfig = default)
    : TestBaseEavCore(testConfig)
{
    protected override IServiceCollection SetupServices(IServiceCollection services) =>
        base.SetupServices(services)
            .AddDataSources();
}