using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSources;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEavDataSource(TestScenario testScenario = default)
    : TestBaseEavCore(testScenario)
{
    protected override IServiceCollection SetupServices(IServiceCollection services) =>
        base.SetupServices(services)
            .AddDataSources();
}