using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.StartUp;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEavCore(EavTestConfig testConfig = default) : TestBaseLib(testConfig)
{
    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services
            .AddEavCore()
            .AddEavCoreFallbackServices();
    }

}