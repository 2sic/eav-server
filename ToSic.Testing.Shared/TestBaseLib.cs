using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib;

namespace ToSic.Testing.Shared;

public abstract class TestBaseLib: TestBaseForIoC
{
    protected TestBaseLib(TestConfiguration testConfiguration = default) : base(testConfiguration)
    {
    }

    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services.AddLibCore();
    }

}