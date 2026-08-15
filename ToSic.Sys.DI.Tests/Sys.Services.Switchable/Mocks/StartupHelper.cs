using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Services.Switchable.Mocks;

public static class StartupHelper
{
    public static IServiceCollection AddMockSwitchableAndCoreServices(this IServiceCollection services)
        => services
            .AddTransient<IMockSwitchableService, MockSwitchableFallback>()
            .AddTransient<IMockSwitchableService, MockSwitchableKeep>()
            .AddTransient<IMockSwitchableService, MockSwitchableSkip>()
            .AddSysCoreLogging() // dependency of service switches
            .AddSysCoreDiServiceSwitchers()
            .AddSysCoreDi();
}