using ToSic.Eav.ImportExport.Tests;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Persistence.Efc.Tests;
using ToSic.Eav.Repositories;
using ToSic.Testing.Shared;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests;

[TestClass]
public class TypeExporter: Efc11TestBase
{
    /// <summary>
    /// Probably set at test-time?
    /// </summary>
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void TypeExp_AllSharedFromInstallation()
    {
        var test = new SpecsTestExportSerialize();

        var loader = GetService<IRepositoryLoader>();
        var app = loader.AppStateReaderRawTac(test.AppId);


        var cts = app.ContentTypes;
        var sharedCts = cts.Where(ct => ct.AlwaysShareConfiguration).ToList();
        var exportStorageRoot = PersistenceTestConstants.ExportStorageRoot(TestContext);
        var fileSysLoader = GetService<FileSystemLoader>().Init(Constants.PresetAppId, exportStorageRoot, RepositoryTypes.TestingDoNotUse, true, null);

        var time = Stopwatch.StartNew();
        sharedCts.ForEach(ct => fileSysLoader.SaveContentType(ct));
        time.Stop();

        Trace.WriteLine("created " + sharedCts.Count + "items and put into " + exportStorageRoot);
        Trace.WriteLine("elapsed: " + time.Elapsed);
    }


}