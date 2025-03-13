using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSources;
using ToSic.Eav.Integration;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEav(TestScenario testScenario = null) : TestBaseEavCore(testScenario)
{
    protected override IServiceCollection SetupServices(IServiceCollection services)
    {
        //base.SetupServices(services);

        // Just add all services, not perfect yet
        // Ideally should only add the services not added by previous layers
        return services
            .AddTransient<FullDbFixtureHelper>()
            .AddDataSources()
            .AddEavEverything();
    }

    /// <summary>
    /// Run configure steps
    /// </summary>
    protected override void Configure()
    {
        // The base has an empty configure, and we want to clearly not call it
        //base.Configure();
        var fixture = GetService<FullDbFixtureHelper>();
        fixture.Configure(TestScenario);
    }

}