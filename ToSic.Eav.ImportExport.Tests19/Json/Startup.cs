using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav.ImportExport.Tests19.Json;

public class Startup : StartupTestFullWithDb
{
    public override void ConfigureServices(IServiceCollection services) =>
        base.ConfigureServices(services
            .AddTransient<JsonTestHelpers>()
        );
}
