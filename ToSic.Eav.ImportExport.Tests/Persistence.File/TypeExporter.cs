using System.Diagnostics;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File;


public class TypeExporter(ITestOutputHelper output, IRepositoryLoader loaderRaw, FileSystemLoader fsLoader) : ServiceBase("test"), IClassFixture<DoFixtureStartup<ScenarioMini>>
{
    [Fact]
    public void TypeExp_AllSharedFromInstallation()
    {
        var test = new SpecsTestExportSerialize();

        //var loader = GetService<IRepositoryLoader>();
        var app = loaderRaw.AppStateReaderRawTac(test.AppId);

        var cts = app.ContentTypes;
        var sharedCts = cts/*.Where(ct => ct.AlwaysShareConfiguration)*/.ToList();
        var exportStorageRoot =
            TestFiles.GetTestPath($"{PersistenceTestConstants.ScenarioRoot}{PersistenceTestConstants.TestingPath3}");// PersistenceTestConstants.ExportStorageRoot(TestContext);
        var fileSysLoader = fsLoader.Init(Constants.PresetAppId, exportStorageRoot, RepositoryTypes.TestingDoNotUse, true, null);

        var time = Stopwatch.StartNew();
        sharedCts.ForEach(fileSysLoader.SaveContentType);
        time.Stop();

        output.WriteLine($"created {sharedCts.Count} items and put into {exportStorageRoot}");
        output.WriteLine($"elapsed: {time.Elapsed}");
    }


}