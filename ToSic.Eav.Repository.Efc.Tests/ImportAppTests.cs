using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Core.Tests;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Repository.Efc.Tests.Mocks;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class ImportAppTests: EavTestBase
    {
        private readonly ZipImport _zipImport;
        private readonly DbDataController _dbData;
        private readonly ZoneManager _zoneManager;

        public ImportAppTests()
        {
            _zipImport = Resolve<ZipImport>();
            _dbData = Resolve<DbDataController>();
            _zoneManager = Resolve<ZoneManager>();
        }

        public static ILog Log = new Log("TstImA");

        //public const string BaseTestPath = @"C:\Projects\eav-server\ToSic.Eav.Repository.Efc.Tests\";
        #region Test Data

        internal class AppImportDef
        {
            public string Name;
            public string Guid;
            public string Zip;
        }
        internal Dictionary<string, AppImportDef> Apps = new Dictionary<string, AppImportDef>()
        {
            { "tile", new AppImportDef() { Name = "Tile", Guid = "efba5a03-2926-488e-a30d-0bf9b6541bbb", Zip = "2sxcApp_Tiles_01.02.00.zip"}},
            { "qr", new AppImportDef() { Name = "QR Code", Guid = "55e57a39-e506-416a-aed0-1c7459d31e86", Zip = "2sxcApp_QRCode_01.00.03.zip"}}

        };

        // just a note: this is Portal 54 or something on Daniels test server, change it to your system if you want to run these tests!
        public const int ZoneId = 56;

        #endregion
        [TestMethod]
        public void DeleteApp_Tile()
        {
            DeleteAnApp(Apps["tile"].Guid);
        }

        [TestMethod]
        public void ImportApp_Tile()
        {
            ImportAnApp("tile");
        }

        [TestMethod]
        public void DeleteApp_Qr()
        {
            DeleteAnApp(Apps["qr"].Guid);
        }
        [TestMethod]
        public void ImportApp_Qr()
        {
            ImportAnApp("qr");
            Console.Write("resolves: " + Factory.CountResolves);
            Console.Write(string.Join("\n", Factory.ResolvesList));
        }

        internal void ImportAnApp(string name)
        { 

            // to be sure, clean up first
            DeleteAnApp(Apps[name].Guid);

            var baseTestPath = new ImportExportEnvironmentMock().BasePath;
            var testFileName = baseTestPath + @"Import-Packages\" + Apps[name].Zip;// 2sxcApp_Tiles_01.02.00.zip";

            bool succeeded;
            var helper = new ImportExportEnvironmentMock();

            using (FileStream fsSource = new FileStream(testFileName, FileMode.Open, FileAccess.Read))
            {
                _zipImport.Init(ZoneId, null, true, null);
                succeeded = _zipImport.ImportZip(fsSource, baseTestPath + @"Temp\");
            }
            Assert.IsTrue(succeeded, "should succeed!");
        }



        public void DeleteAnApp(string appGuid)
        {
            var applist = _dbData.Init(ZoneId, null, Log).SqlDb.ToSicEavApps.Where(a => a.ZoneId == ZoneId).ToList();
            var appId = applist.FirstOrDefault(a => a.Name == appGuid)?.AppId ?? 0;
            if (appId > 0) _zoneManager.Init(ZoneId, Log).DeleteApp(appId);

        }
    }
}
