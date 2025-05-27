using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Platform;
using ToSic.Testing.Shared.Platforms;

namespace ToSic.Eav.FeatureTests;

public class IFeaturesInternalTests(ISysFeaturesService featuresInternal) : IClassFixture<DoFixtureStartup<ScenarioFullPatronsWithDb>>
{
    public class Startup : StartupTestsApps
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddTransient<IPlatformInfo, TestPlatformPatronPerfectionist>();
        }
    }


    [Fact]
    public void LoadFromConfiguration()
    {
        var x = featuresInternal.All;
        True(x.Count() > 2, "expect a few features in configuration");

    }

    [Fact]
    public void PasteClipboardActive()
    {
        var x = featuresInternal.IsEnabled(BuiltInFeatures.PasteImageFromClipboard.Guid);
        True(x, "this should be enabled and non-expired");
    }

    [Fact]
    public void InventedFeatureGuid()
    {
        var inventedGuid = new Guid("12345678-1c8b-4286-a33b-3210ed3b2d9a");
        var x = featuresInternal.IsEnabled(inventedGuid);
        False(x, "this should be enabled and expired");
    }
}