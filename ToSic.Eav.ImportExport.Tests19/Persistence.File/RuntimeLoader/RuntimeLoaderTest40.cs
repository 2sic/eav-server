using System.Diagnostics;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File.RuntimeLoader;


public class RuntimeLoaderTest40(IAppReaderFactory appReaderFactor, ITestOutputHelper output) : IClassFixture<DoFixtureStartup<Scenario40TypesInSingleFiles>>
{
    private const int ExpectedTypesSysAndJson = 40;

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