using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.StartUp;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Lib;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEavCore(TestScenario testScenario = null) : TestBaseForIoC(testScenario)
{
    protected override IServiceCollection SetupServices(IServiceCollection services) =>
        base.SetupServices(services)
            .AddLibCore()
            .AddEavCore()
            .AddEavCoreFallbackServices();
}