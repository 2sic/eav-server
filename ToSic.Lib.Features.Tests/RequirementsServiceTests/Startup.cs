using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.Internal.FeatSys;

namespace ToSic.Lib.Features.Tests.RequirementsServiceTests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddLibFeatSys()
            .AddLibCore();
}