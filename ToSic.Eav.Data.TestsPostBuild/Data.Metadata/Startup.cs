using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav.Data.Metadata;

public class Startup : StartupTestsEavDataBuild
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<EntityWithMetadataGenerator>();
        base.ConfigureServices(services);
    }
}
