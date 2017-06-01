using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.ImportExport;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class ExportImportDataTest
    {
        [TestMethod]
        [Ignore] // ignor for now, reason is that we don't have a mock-portal-settings provider
        public void ExportData()
        {
            var zoneId = 2;
            var appId = 2;
            //string[] contentTypeIdsString = null;
            //string[] entityIdsString = null;
            var fileXml = Factory.Resolve<XmlExporter>().Init(zoneId, appId, false,
                /*contentTypeIdsString?.Split(';') ?? */new string[0],
                /*entityIdsString?.Split(';') ?? */new string[0]
            ).GenerateNiceXml();

        }
    }
}
