using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.StartupTests;

namespace ToSic.Eav.DataSources.Attributes;

internal class StartupTestsAttributeRename: StartupTestsEavCoreAndDataSources
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddTransient<AttributeRenameTester>();
    }
}