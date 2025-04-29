using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav.DataSources.Attributes;

internal class StartupTestsAttributeRename: StartupCoreDataSourcesAndTestData
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddTransient<AttributeRenameTester>();
    }
}