using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.WithFactory;

public class Startup : StartupTestsEavDataBuild
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTransient<MockDataGenerator>()
            .AddTransient<MockModelRequiringFactoryWithDependencies.Dependencies>();
        
        base.ConfigureServices(services);
    }
}
