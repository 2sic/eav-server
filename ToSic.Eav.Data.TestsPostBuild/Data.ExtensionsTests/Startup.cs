using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Data.ExtensionsTests.TestData;

namespace ToSic.Eav.Data.ExtensionsTests;

public class Startup : StartupTestsEavDataBuild
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTransient<TestDataGenerator>()
            .AddTransient<TestModelDependency>();
        base.ConfigureServices(services);
    }
}
