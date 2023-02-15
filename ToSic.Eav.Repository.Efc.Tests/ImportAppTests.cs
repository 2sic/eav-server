using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc.Tests.Mocks;
using ToSic.Sxc.Apps.ImportExport;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class ImportAppTests: TestBaseDiEavFullAndDb
    {
        private readonly ZipImport _zipImport;
        private readonly DbDataController _dbData;
        private readonly ZoneManager _zoneManager;

        public ImportAppTests()
        {
            _zipImport = Build<ZipImport>();
            _dbData = Build<DbDataController>();
            _zoneManager = Build<ZoneManager>();
        }

        protected override void AddServices(IServiceCollection services)
        {
            services.AddTransient<IImportExportEnvironment, ImportExportEnvironmentMock>();
            services.AddTransient<XmlImportWithFiles, XmlImportFull>();
        }

        //public const string BaseTestPath = @"C:\Projects\eav-server\ToSic.Eav.Repository.Efc.Tests\";
        #region Test Data

        internal class AppImportDef
        {
            public string Name;
            public string Guid;
            public string Zip;
        }
        internal Dictionary<string, AppImportDef> Apps = new Dictionary<string, AppImportDef>
        {
            { "tile", new AppImportDef { Name = "Tile", Guid = "efba5a03-2926-488e-a30d-0bf9b6541bbb", Zip = "2sxcApp_Tiles_01.02.00.zip"}},
            { "qr", new AppImportDef { Name = "QR Code", Guid = "55e57a39-e506-416a-aed0-1c7459d31e86", Zip = "2sxcApp_QRCode_01.00.03.zip"}}

        };

        // Zone 5 is the /import-export on the eav-testing system
        public const int ZoneId = 5;

        #endregion
        [TestMethod]
        public void DeleteApp_Tile()
        {
            DeleteAnApp(Apps["tile"].Guid);
        }

        [TestMethod]
        public void ImportApp_Tile() => ImportAnApp("tile");

        [TestMethod]
        public void DeleteApp_Qr() => DeleteAnApp(Apps["qr"].Guid);

        [TestMethod]
        public void ImportApp_Qr() => ImportAnApp("qr");

        internal void ImportAnApp(string name)
        {
            // to be sure, clean up first
            DeleteAnApp(Apps[name].Guid);

            var helper = (ImportExportEnvironmentMock)Build<IImportExportEnvironment>();
            var baseTestPath = helper.BasePath;
            var testFileName = baseTestPath + @"Import-Packages\" + Apps[name].Zip;

            bool succeeded;

            using (FileStream fsSource = new FileStream(testFileName, FileMode.Open, FileAccess.Read))
            {
                _zipImport.Init(ZoneId, null, true);
                succeeded = _zipImport.ImportZip(fsSource, baseTestPath + @"Temp\");
            }
            Assert.IsTrue(succeeded, "should succeed!");
        }



        public void DeleteAnApp(string appGuid)
        {
            var applist = _dbData.Init(ZoneId, null).SqlDb.ToSicEavApps.Where(a => a.ZoneId == ZoneId).ToList();
            var appId = applist.FirstOrDefault(a => a.Name == appGuid)?.AppId ?? 0;
            if (appId > 0) _zoneManager.SetId(ZoneId).DeleteApp(appId, true);

        }
    }
}
