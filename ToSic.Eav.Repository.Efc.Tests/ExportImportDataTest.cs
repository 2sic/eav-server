using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class ExportImportDataTest
    {
        [TestMethod]
        [Ignore] // ignor for now, reason is that we don't have a mock-portal-settings provider
        public void ExportData()
        {
            var Log = new Log("TstExp");
            var zoneId = 2;
            var appId = 2;
            var appRuntime = new AppRuntime(appId, Log);

            //string[] contentTypeIdsString = null;
            //string[] entityIdsString = null;
            var fileXml = Factory.Resolve<XmlExporter>().Init(zoneId, appId, appRuntime, false,
                /*contentTypeIdsString?.Split(';') ?? */new string[0],
                /*entityIdsString?.Split(';') ?? */new string[0],
                Log
            ).GenerateNiceXml();

        }
    }
}
