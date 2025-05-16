using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Internal.Requirements;
using ToSic.Lib.Features.Tests;
using ToSic.Lib.Features.Tests.RequirementChecks.Mocks;

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