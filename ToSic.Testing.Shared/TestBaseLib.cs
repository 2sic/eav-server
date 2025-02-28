using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib;

namespace ToSic.Testing.Shared;

public abstract class TestBaseLib(EavTestConfig testConfig = default) : TestBaseForIoC(testConfig)
{
    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services.AddLibCore();
    }

}