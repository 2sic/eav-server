using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Internal.Requirements;
using ToSic.Lib.Features.Tests.RequirementChecks.Mocks;

namespace ToSic.Lib.Features.Tests.RequirementsServiceTests.AnimalRequirements;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        new RequirementsServiceTests.Startup().ConfigureServices(services);
        services
            .AddTransient<IRequirementCheck, MockAnimalRequirementsCheck>();
    }
}