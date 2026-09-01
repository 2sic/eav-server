using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Services.Generator.WithOptions;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddTransient<MockServiceWithSetup>()
            .AddTransient<MockServiceSpawnNewCustomOptions>()
            .AddTransient<MockServiceSpawnNewDefaultOptions>()
            .AddSysCore();
}