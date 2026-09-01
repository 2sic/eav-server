using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.TestHelpers.Equality;

namespace ToSic.Eav.Models.Equality;

public class Startup : Models.Startup
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services.AddEqualityChecker());
    }
}