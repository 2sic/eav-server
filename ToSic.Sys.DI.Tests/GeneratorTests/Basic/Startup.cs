using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Lib.DI.GeneratorTests.Basic;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddTransient<TestObjectToGenerate>()
            .AddLibCore();
}