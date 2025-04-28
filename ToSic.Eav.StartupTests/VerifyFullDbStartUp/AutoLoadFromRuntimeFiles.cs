using ToSic.Eav.Apps;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.VerifyFullDbStartUp;

[Startup(typeof(StartupTestsApps))]
public class AutoLoadFromRuntimeFiles(IAppReaderFactory appReaderFactory) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    // status 2021-11-04 is 77 files
    private int TypesInFileRuntimeMin = 75;
    private int TypesInFileRuntimeMax = 150;

    [Fact]
    public void ScanForTypesFileBased()
    {
        var globalApp = appReaderFactory.GetSystemPresetTac();
        var types = globalApp.ContentTypes;
        var count = types.Count();
        True(TypesInFileRuntimeMin < count
                      && TypesInFileRuntimeMax > count,
            $"expect a fixed about of types at dev time - got {count}");
    }
}