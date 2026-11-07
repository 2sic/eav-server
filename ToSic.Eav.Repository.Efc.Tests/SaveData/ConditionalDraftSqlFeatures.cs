using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Repository.Efc.Tests.SaveData;

// Switch only the save-pipeline choice, in this test host. No feature/license files are changed.
public sealed class ConditionalDraftSqlFeatures(FeaturesCatalog catalog) : LibSysFeaturesService(catalog), ISysFeaturesService
{
    public bool Optimized { get; set; }
    public int SaveModeChecks { get; private set; }

    bool ISysFeaturesService.IsEnabled(params Feature[] features)
        => features.All(f =>
        {
            if (f.Guid != BuiltInFeatures.DatabaseWriteOptimized.Guid)
                return base.IsEnabled(f);
            SaveModeChecks++;
            return Optimized;
        });

    public static void Configure(IServiceCollection services)
    {
        services.AddSingleton<ConditionalDraftSqlFeatures>();
        services.Replace(ServiceDescriptor.Singleton<ISysFeaturesService>(sp => sp.GetRequiredService<ConditionalDraftSqlFeatures>()));
    }
}

public class StartupConditionalDraftSql : StartupTestsApps
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        ConditionalDraftSqlFeatures.Configure(services);
    }
}
