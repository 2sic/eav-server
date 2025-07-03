using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.DI.SwitchableServices.Services;
using ToSic.Sys.Startup;

namespace ToSic.Lib.DI.SwitchableServices;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddTransient<ITestSwitchableService, TestSwitchableFallback>()
            .AddTransient<ITestSwitchableService, TestSwitchableKeep>()
            .AddTransient<ITestSwitchableService, TestSwitchableSkip>()
            .AddSysCore();
}