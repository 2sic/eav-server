using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;

namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddTransient<MockServiceWithSetup>()
            .AddTransient<MockServiceSpawnNewCustomOptions>()
            .AddTransient<MockServiceSpawnNewDefaultOptions>()
            .AddSysCore();
}