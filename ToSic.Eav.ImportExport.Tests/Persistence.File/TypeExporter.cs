using System.Diagnostics;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File;


public class TypeExporter(ITestOutputHelper output, IAppsAndZonesLoaderWithRaw loaderRaw, FileSystemLoader fsLoader) : ServiceBase("test"), IClassFixture<DoFixtureStartup<ScenarioMini>>
{
    [Fact]
    public void TypeExp_AllSharedFromInstallation()
    {
        var test = new SpecsTestExportSerialize();

        var app = loaderRaw.AppStateReaderRawTac(test.AppId);

        var cts = app.ContentTypes;
        var sharedCts = cts.ToList();
        var exportStorageRoot =
            TestFiles.GetTestPath($"{PersistenceTestConstants.ScenarioRoot}{PersistenceTestConstants.TestingPath3}");
        var fileSysLoader = fsLoader;
        fileSysLoader.Setup(new()
        {
            AppId = KnownAppsConstants.PresetAppId,
            Path = exportStorageRoot,
            RepoType = RepositoryTypes.TestingDoNotUse,
            IgnoreMissing = true,
            EntitiesSource = null
        });

        var time = Stopwatch.StartNew();
        sharedCts.ForEach(fileSysLoader.SaveContentType);
        time.Stop();

        output.WriteLine($"created {sharedCts.Count} items and put into {exportStorageRoot}");
        output.WriteLine($"elapsed: {time.Elapsed}");
    }


}