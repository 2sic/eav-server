using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

public class Startup : StartupTestsEavDataBuild
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTransient<TestDataGenerator>()
            .AddTransient<TestModelRequiringFactory.TestModelDependencyInjection>();
        base.ConfigureServices(services);
    }
}
