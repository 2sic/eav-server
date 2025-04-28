using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File.RuntimeLoader;

// Note: Sometimes in parallel executions this may end up picking up more than the expected number of types
// So then just re-run by itself to verify
// Someday improve testing configuration that it's always right...

public class RuntimeLoaderTest5(IAppReaderFactory appReaderFactor, ITestOutputHelper output) : IClassFixture<DoFixtureStartup<ScenarioMini>>
{
    private const int ExpectedTypesSysAndJson = 8;

    [Fact(Skip = "This test isn't properly designed or updated to any realistic data")]
    public void TestWith3FileTypes() =>
        RuntimeLoaderTestHelper.TestWith3FileTypes(appReaderFactor.GetSystemPresetTac(), output, ExpectedTypesSysAndJson);

    [Fact]
    public void TestWith40FileTypes_JustReRunIfItFails() =>
        RuntimeLoaderTestHelper.TestWithXContentTypes(appReaderFactor.GetSystemPresetTac(), output, ExpectedTypesSysAndJson, ExpectedTypesSysAndJson);


    [Fact]
    public void TestConcurrentInitialize() =>
        RuntimeLoaderTestHelper.TestConcurrentInitializeGlobalAppState(appReaderFactor.GetSystemPresetTac(), output, ExpectedTypesSysAndJson);

}