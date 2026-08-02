using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Inheritances;


public abstract class StartupBase : StartupTestsEavDataBuild
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTransient<MockDataGenerator>()
            .AddTransient(typeof(MockDataGenerator<>))
            .AddTransient<ToModelInheritanceTests.TestCaseGenerator>();
        
        // Try to set a default value for the ToModel provider
        services.TryAddTransient<IToModelTac, ToModelTacPublic>();

        base.ConfigureServices(services);
    }
}