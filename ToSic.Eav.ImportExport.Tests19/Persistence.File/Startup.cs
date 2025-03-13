using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.ImportExport.Tests19.Json;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File;

public class Startup : StartupTestFullWithDb
{
    public override void ConfigureServices(IServiceCollection services) =>
        base.ConfigureServices(services
            .AddTransient<JsonTestHelpers>()
        );
}