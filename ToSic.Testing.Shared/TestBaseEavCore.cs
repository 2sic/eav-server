using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.StartUp;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEavCore: TestBaseLib
{
    protected TestBaseEavCore(TestConfiguration testConfiguration = default) : base(testConfiguration)
    {
    }

    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services
            .AddEavCore()
            .AddEavCoreFallbackServices();
    }

}