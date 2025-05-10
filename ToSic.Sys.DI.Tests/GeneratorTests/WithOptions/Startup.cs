using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddTransient<ServiceWithOwnOptions>()
            .AddTransient<ServiceWithDefaultOptions>()
            .AddLibCore();
}