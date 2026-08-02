using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Inheritances;

public class Startup : StartupTestsEavDataBuild
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTransient<MockDataGenerator>()
            .AddTransient(typeof(MockDataGenerator<>))
            .AddTransient<ToModelInheritanceTests.TestCaseGenerator>();
        
        base.ConfigureServices(services);
    }
}
