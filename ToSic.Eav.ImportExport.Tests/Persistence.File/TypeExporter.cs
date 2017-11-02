using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Enums;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Tests;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests
{
    [TestClass]
    public class TypeExporter: PersistenceTestsBase
    {

        [TestMethod]
        public void TypeExp_AllSharedFromInstallation()
        {
            var test = new TestValuesOnPc2Dm();
            var dbc = DbDataController.Instance(null, test.AppId, Log);


            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppPackage(test.RootAppId);


            var cts = app.ContentTypes;
            var sharedCts = cts.Where(ct => (ct as ContentType).AlwaysShareConfiguration).ToList();

            var fileSysLoader = new FileSystemLoader(ExportStorageRoot, Repositories.TestFiles, true, Log);

            var time = Stopwatch.StartNew();
            sharedCts.ForEach(ct => fileSysLoader.SaveContentType(ct));
            time.Stop();

            Trace.WriteLine("created " + sharedCts.Count + "items and put into " + ExportStorageRoot);
            Trace.WriteLine("elapsed: " + time.Elapsed);
        }


    }
}
