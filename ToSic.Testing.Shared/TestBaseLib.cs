using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib;

namespace ToSic.Testing.Shared;

public abstract class TestBaseLib(TestConfiguration testConfiguration = default) : TestBaseForIoC(testConfiguration)
{
    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services.AddLibCore();
    }

}