using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;

namespace ToSic.Lib.DI.GeneratorTests.Basic;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddTransient<TestObjectToGenerate>()
            .AddSysCore();
}