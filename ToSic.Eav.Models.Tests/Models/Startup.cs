using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

public class Startup : StartupTestsEavDataBuild
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTransient<MockDataGenerator>()
            .AddTransient(typeof(MockDataGenerator<>));
        
        // Try to set a default value for the ToModel provider
        services.TryAddTransient<IToModelTac, ToModelTacPublic>();
        
        base.ConfigureServices(services);
    }
}
