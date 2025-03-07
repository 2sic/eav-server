using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.StartUp;
using ToSic.Eav.Testing;
using ToSic.Lib;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEavCore(EavTestConfig testConfig = default) : TestBaseForIoC(testConfig)
{
    protected override IServiceCollection SetupServices(IServiceCollection services) =>
        base.SetupServices(services)
            .AddLibCore()
            .AddEavCore()
            .AddEavCoreFallbackServices();
}