using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Apps.Tests.ImportExport
{
    [TestClass]
    public class XmlTable: EavTestBase
    {

        public static ILog Log = new Log("TstXml");
        private int AppId = 78;

        [TestMethod]
        [Ignore]
        public void XmlTable_ResolveWithFullFallback()
        {
            var exporter = BuildExporter(AppId, "BlogPost");

            // todo: need ML portal for testing

        }



        private ExportListXml BuildExporter(int appId, string ctName)
        {
            var loader = Resolve<Efc11Loader>();
            var appPackage = loader.AppState(appId, false);
            var type = appPackage.ContentTypes.First(ct => ct.Name == ctName);
            return Resolve<ExportListXml>().Init(appPackage, type, Log);
        }
    }
}
