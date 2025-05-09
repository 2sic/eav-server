using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Emit;

namespace ToSic.Lib.DI.GeneratorTests.NeedsOptions;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
    services
            .AddTransient(typeof(IGenerator<,>), typeof(Generator<,>))
            .AddTransient<TestNeedingOptions>()
            .AddLibCore();
}