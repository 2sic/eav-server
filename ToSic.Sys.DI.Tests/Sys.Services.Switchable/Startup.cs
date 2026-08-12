using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;
using ToSic.Sys.Services.Switchable.Mocks;

namespace ToSic.Sys.Services.Switchable;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddTransient<IMockSwitchableService, MockSwitchableFallback>()
            .AddTransient<IMockSwitchableService, MockSwitchableKeep>()
            .AddTransient<IMockSwitchableService, MockSwitchableSkip>()
            .AddSysCore();
}