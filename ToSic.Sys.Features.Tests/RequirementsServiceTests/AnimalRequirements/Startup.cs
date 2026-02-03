using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Features.Tests.RequirementChecks.Mocks;
using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features.Tests.RequirementsServiceTests.AnimalRequirements;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .StartupLibFeaturesTests()
            .AddTransient<IRequirementCheck, MockAnimalRequirementsCheck>();
    }
}