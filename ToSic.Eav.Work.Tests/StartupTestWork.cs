using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav;

public class StartupTestWork: StartupTestsApps
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services
            .AddEavWork()
            .AddWorkFallbackServices();
    }
}
