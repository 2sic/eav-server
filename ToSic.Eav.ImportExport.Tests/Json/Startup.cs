using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav.ImportExport.Tests.Json;

public class Startup : StartupTestFullWithDb
{
    public override void ConfigureServices(IServiceCollection services) =>
        base.ConfigureServices(services
            .AddTransient<JsonTestHelpers>()
        );
}
