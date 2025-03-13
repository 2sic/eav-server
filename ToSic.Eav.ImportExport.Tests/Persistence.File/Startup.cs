using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.ImportExport.Tests.Json;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

public class Startup : StartupTestFullWithDb
{
    public override void ConfigureServices(IServiceCollection services) =>
        base.ConfigureServices(services
            .AddTransient<JsonTestHelpers>()
        );
}