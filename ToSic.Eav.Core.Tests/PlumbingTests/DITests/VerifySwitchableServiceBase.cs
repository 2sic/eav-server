using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.PlumbingTests.DITests;

public abstract class VerifySwitchableServiceBase : TestBaseForIoC
{
    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services.AddTransient<ITestSwitchableService, TestSwitchableFallback>();
        services.AddTransient<ITestSwitchableService, TestSwitchableKeep>();
        services.AddTransient<ITestSwitchableService, TestSwitchableSkip>();
        services
            .AddLibCore();
    }

}