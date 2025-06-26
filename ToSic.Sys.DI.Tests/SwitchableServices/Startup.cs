using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.DI.SwitchableServices.Services;

namespace ToSic.Lib.DI.SwitchableServices;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddTransient<ITestSwitchableService, TestSwitchableFallback>()
            .AddTransient<ITestSwitchableService, TestSwitchableKeep>()
            .AddTransient<ITestSwitchableService, TestSwitchableSkip>()
            .AddLibCore();
}