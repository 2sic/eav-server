using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.PlumbingTests.DITests;

public abstract class VerifySwitchableServiceBase : TestBaseForIoC
{
    protected override IServiceCollection SetupServices(IServiceCollection services) =>
        base.SetupServices(services)
            .AddTransient<ITestSwitchableService, TestSwitchableFallback>()
            .AddTransient<ITestSwitchableService, TestSwitchableKeep>()
            .AddTransient<ITestSwitchableService, TestSwitchableSkip>()
            .AddLibCore();
}