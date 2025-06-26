using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.Features.Tests;
using ToSic.Lib.Features.Tests.RequirementChecks.Mocks;
using ToSic.Sys.Requirements;

namespace ToSic.Lib.RequirementsServiceTests.AnimalRequirements;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .StartupLibFeaturesTests()
            .AddTransient<IRequirementCheck, MockAnimalRequirementsCheck>();
    }
}