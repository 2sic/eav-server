using System.Diagnostics;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc.Tests;
using ToSic.Testing.Shared;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests;

[TestClass]
public class TypeExporter: PersistenceTestsBase
{

    [TestMethod]
    public void TypeExp_AllSharedFromInstallation()
    {
        var test = new SpecsTestExportSerialize();

        var loader = GetService<IRepositoryLoader>();
        var app = loader.AppStateReaderRawTac(test.AppId);


        var cts = app.ContentTypes;
        var sharedCts = cts.Where(ct => ct.AlwaysShareConfiguration).ToList();
        var fileSysLoader = GetService<FileSystemLoader>().Init(Constants.PresetAppId, ExportStorageRoot, RepositoryTypes.TestingDoNotUse, true, null);

        var time = Stopwatch.StartNew();
        sharedCts.ForEach(ct => fileSysLoader.SaveContentType(ct));
        time.Stop();

        Trace.WriteLine("created " + sharedCts.Count + "items and put into " + ExportStorageRoot);
        Trace.WriteLine("elapsed: " + time.Elapsed);
    }


}