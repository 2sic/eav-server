using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Lib.Logging.Simple;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class ExportImportDataTest: TestBaseDiEavFullAndDb
    {
        private readonly AppRuntime _appRuntime;
        private readonly XmlExporter _xmlExporter;

        public ExportImportDataTest()
        {
            _appRuntime = Build<AppRuntime>();
            _xmlExporter = Build<XmlExporter>();
        }
        [TestMethod]
        [Ignore] // ignore for now, reason is that we don't have a mock-portal-settings provider
        public void ExportData()
        {
            var Log = new Log("TstExp");
            var zoneId = 2;
            var appId = 2;
            var appRuntime = _appRuntime.Init(appId, true, Log);

            var fileXml = _xmlExporter.Init(zoneId, appId, appRuntime, false,
                /*contentTypeIdsString?.Split(';') ?? */new string[0],
                /*entityIdsString?.Split(';') ?? */new string[0],
                Log
            ).GenerateNiceXml();

        }
    }
}
