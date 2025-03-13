using System.Diagnostics;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File.RuntimeLoader;

public class RuntimeLoader400ContentTypes(IAppLoader appLoader, ITestOutputHelper output) : IClassFixture<DoFixtureStartup<ScenarioMini>>
{
    /// <summary>
    /// Just a test that tries to load data from a folder with 40 content types - 10x.
    /// It does not really rely on the automatic data loading, so it's quite independent.
    /// </summary>
    [Fact]
    public void TimingWith400FileTypes()
    {
        // set loader root path, based on test environment
        AdditionalGlobalFolderRepositoryForReflection.PathToUse = TestFiles.GetTestPath(PersistenceTestConstants.Scenario40Types + "\\App_Data\\system");
        var loader = (AppLoader)appLoader;
        var time = Stopwatch.StartNew();

        output.WriteLine($"Note: ATM the first loader is the {nameof(AdditionalGlobalFolderRepositoryForReflection)} - but that is a coincidence, so this test may need to be adjusted in future");
        var firstLoader = loader.Loaders.First();
        
        for (var i = 0; i < 10; i++)
        {
            var cts = firstLoader.ContentTypes();
            Equal(40, cts.Count); // Test scenario Dnn has 40 types
            output.WriteLine($"time after cycle {i} was {time.Elapsed}; found {cts.Count}");
        }
        time.Stop();

        output.WriteLine("time used to load 400 with debug/testing overhead: " + time.Elapsed);
    }

}