using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Tests.ImportExport
{
    [TestClass]
    public class XmlTable
    {

        public static Log Log = new Log("TstXml");
        private int AppId = 78;
        private int TestItemId = 0;

        private const string NonLinkType = "whatever";

        string link2sic = "http://www.2sic.com/", 
            linkFile = "file:304", 
            linkPage = "page:44";

        string unchanged = "some test string which shouldn't change in a resolve as it's not a link or reference";

        [TestMethod]
        [Ignore]
        public void XmlTable_ResolveWithFullFallback()
        {
            var exporter = BuildExporter(AppId, "BlogPost");

            // todo: need ML portal for testing

        }

        #region test basic ResolveHyperlink and ResolveValue
        [TestMethod]
        public void XmlTable_ResolveHyperlink()
        {
            var exporter = BuildExporter(AppId, "BlogPost");
            
            // test the Resolve Hyperlink
            string link = "";
            link = exporter.ResolveHyperlinksFromTennant(Constants.Hyperlink, link2sic);
            Assert.AreEqual(link, link2sic, "real link should stay the same");

            link = exporter.ResolveHyperlinksFromTennant(Constants.Hyperlink, linkFile);
            Assert.AreNotEqual(link, linkFile, "file link should change");

            link = exporter.ResolveHyperlinksFromTennant(Constants.Hyperlink, linkPage);
            Assert.AreNotEqual(link, linkPage, "page link should change");

            link = exporter.ResolveHyperlinksFromTennant(NonLinkType, "http://www.2sic.com/");
            Assert.AreEqual(link, link2sic, "non-link shouldn't resolve");

            link = exporter.ResolveHyperlinksFromTennant(NonLinkType, linkPage);
            Assert.AreEqual(link, linkPage, "non-link shouldn't resolve");
            link = exporter.ResolveHyperlinksFromTennant(NonLinkType, linkFile);
            Assert.AreEqual(link, linkFile, "non-link shouldn't resolve");
        }

        [TestMethod]
        public void XmlTable_ResolveValue()
        {
            var exporter = BuildExporter(AppId, "BlogPost");

            // test resolves on any value, these versions should always resolve to default values but not resolve links
            TestResolvesWithNonLinkType(exporter, NonLinkType, false);
            TestResolvesWithNonLinkType(exporter, NonLinkType, true);
            TestResolvesWithNonLinkType(exporter, Constants.Hyperlink, false);

            var type = Constants.Hyperlink;
            // test resolves on any value, just certainly not a link, with "no-resolve"
            Assert.AreEqual(XmlConstants.Null, exporter.ResolveValue(type, null, true), "test null resolve");
            Assert.AreEqual(XmlConstants.Empty, exporter.ResolveValue(type, "", true), "test empty resolve");
            Assert.AreEqual(unchanged, exporter.ResolveValue(type, unchanged, true), "test text resolve");
            Assert.AreEqual(link2sic, exporter.ResolveValue(type, link2sic, true), "test http: resolve");
            Assert.AreNotEqual(linkFile, exporter.ResolveValue(type, linkFile, true), "test file: resolve");
            Assert.AreNotEqual(linkPage, exporter.ResolveValue(type, linkPage, true), "test page: resolve");
        }

        private void TestResolvesWithNonLinkType(ExportListXml exp, string attrType, bool tryResolve)
        {
            Assert.AreEqual(XmlConstants.Null, exp.ResolveValue(attrType, null, tryResolve), "test null resolve");
            Assert.AreEqual(XmlConstants.Empty, exp.ResolveValue(attrType, "", tryResolve), "test empty resolve");
            Assert.AreEqual(unchanged, exp.ResolveValue(attrType, unchanged, tryResolve), "test text resolve");
            Assert.AreEqual(link2sic, exp.ResolveValue(attrType, link2sic, tryResolve), "test http: resolve");
            Assert.AreEqual(linkFile, exp.ResolveValue(attrType, linkFile, tryResolve), "test file: resolve");
            Assert.AreEqual(linkPage, exp.ResolveValue(attrType, linkPage, tryResolve), "test page: resolve");
        }

        #endregion


        private ExportListXml BuildExporter(int appId, string ctName)
        {
            var dbc = DbDataController.Instance(null, appId, Log);
            var loader = new Efc11Loader(dbc.SqlDb);
            var appPackage = loader.AppPackage(appId);
            var type = appPackage.ContentTypes.First(ct => ct.Name == ctName);
            return new ExportListXml(appPackage, type, Log);
        }
    }
}
